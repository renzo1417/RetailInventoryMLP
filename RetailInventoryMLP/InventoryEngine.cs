using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using Backprop;

namespace RetailInventoryMLP
{
    /// Represents an inventory item record containing raw inputs, normalized inputs, and classification targets.
    public class InventoryRow
    {
        public double[] RawInputs = new double[4];
        public double[] NormInputs = new double[4];
        public double BinaryTarget;
        public double[] MultiClassTarget = new double[3];
        public double[] MultiLabelTarget = new double[3];
    }

    /// Core engine handling data preprocessing, anti-leakage normalization, neural network training, and evaluation.
    public class InventoryEngine
    {
        public List<InventoryRow> AllRows { get; private set; } = new List<InventoryRow>();
        public List<InventoryRow> TrainSet { get; private set; } = new List<InventoryRow>();
        public List<InventoryRow> TestSet { get; private set; } = new List<InventoryRow>();

        public double[] MinValues { get; private set; } = new double[4];
        public double[] MaxValues { get; private set; } = new double[4];

        public NeuralNet BinaryNet { get; private set; }
        public NeuralNet MultiClassNet { get; private set; }
        public NeuralNet MultiLabelNet { get; private set; }

        public DataTable DataPreviewTable { get; private set; } = new DataTable();
        public DataTable MetricsTable { get; private set; } = new DataTable();
        public DataTable EpochLogTable { get; private set; } = new DataTable();
        public DataTable TuningTable { get; private set; } = new DataTable();

        public bool IsDataLoaded { get { return AllRows.Count > 0 && TrainSet.Count > 0; } }
        public bool AreModelsTrained { get { return BinaryNet != null && MultiClassNet != null && MultiLabelNet != null; } }

        public InventoryEngine()
        {
            InitTables();
        }

        private void InitTables()
        {
            DataPreviewTable.Columns.Add("Item #", typeof(int));
            DataPreviewTable.Columns.Add("Partition", typeof(string));
            DataPreviewTable.Columns.Add("Stock (Raw / Norm)", typeof(string));
            DataPreviewTable.Columns.Add("Sales (Raw / Norm)", typeof(string));
            DataPreviewTable.Columns.Add("Expiry (Raw / Norm)", typeof(string));
            DataPreviewTable.Columns.Add("Margin (Raw / Norm)", typeof(string));
            DataPreviewTable.Columns.Add("Restock (Binary)", typeof(int));
            DataPreviewTable.Columns.Add("Velocity Tier (Multi-Class)", typeof(string));
            DataPreviewTable.Columns.Add("Tags (Multi-Label)", typeof(string));

            MetricsTable.Columns.Add("Classification Task", typeof(string));
            MetricsTable.Columns.Add("Network Size", typeof(string));
            MetricsTable.Columns.Add("Test Loss (MSE)", typeof(string));
            MetricsTable.Columns.Add("Accuracy", typeof(string));
            MetricsTable.Columns.Add("Precision", typeof(string));
            MetricsTable.Columns.Add("Recall", typeof(string));

            EpochLogTable.Columns.Add("Task", typeof(string));
            EpochLogTable.Columns.Add("Epoch", typeof(int));
            EpochLogTable.Columns.Add("Training Loss (MSE)", typeof(string));

            TuningTable.Columns.Add("Trial", typeof(string));
            TuningTable.Columns.Add("Hidden Neurons", typeof(int));
            TuningTable.Columns.Add("Learning Rate", typeof(string));
            TuningTable.Columns.Add("Epochs", typeof(int));
            TuningTable.Columns.Add("Final Train MSE", typeof(string));
            TuningTable.Columns.Add("Test Accuracy", typeof(string));
            TuningTable.Columns.Add("Remarks", typeof(string));
        }

        /// Generates a reproducible synthetic retail dataset if the specified CSV does not exist.
        public void EnsureDatasetExists(string csvPath, int rowCount = 200)
        {
            if (File.Exists(csvPath)) return;

            string dir = Path.GetDirectoryName(csvPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Random rng = new Random(42);
            using (StreamWriter sw = new StreamWriter(csvPath))
            {
                sw.WriteLine("StockQty,DailySales,DaysToExpiry,ProfitMargin,RestockAlert,Tier_Slow,Tier_Regular,Tier_Fast,Tag_PromoDiscount,Tag_ExpiryRisk,Tag_HighProfit");

                for (int i = 0; i < rowCount; i++)
                {
                    double stock = rng.Next(5, 201);
                    double sales = rng.Next(1, 51);
                    double expiry = rng.Next(3, 181);
                    double margin = rng.Next(5, 61);

                    int restock = (stock < 75 && sales > 20) || (stock < 35) ? 1 : 0;

                    int tSlow = 0, tReg = 0, tFast = 0;
                    if (sales < 18) tSlow = 1;
                    else if (sales < 35) tReg = 1;
                    else tFast = 1;

                    int tagPromo = (stock > 100 && sales < 25) ? 1 : 0;
                    int tagExpiry = (expiry < 60) ? 1 : 0;
                    int tagProfit = (margin >= 32) ? 1 : 0;

                    sw.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                        stock, sales, expiry, margin, restock, tSlow, tReg, tFast, tagPromo, tagExpiry, tagProfit));
                }
            }
        }

        /// Loads CSV data, performs deterministic shuffle, splits into 80% train and 20% test,
        /// and fits MinMax scaler STRICTLY on TrainSet to prevent data leakage.
        public void LoadAndPreprocessCsv(string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                EnsureDatasetExists(csvPath, 200);
            }

            string[] lines = File.ReadAllLines(csvPath);

            AllRows.Clear();
            DataPreviewTable.Rows.Clear();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string[] cols = lines[i].Split(',');
                if (cols.Length < 5) continue;

                InventoryRow row = new InventoryRow();
                bool parseOk = true;

                for (int c = 0; c < 4; c++)
                {
                    double val;
                    if (double.TryParse(cols[c].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                    {
                        row.RawInputs[c] = val;
                    }
                    else
                    {
                        parseOk = false;
                        break;
                    }
                }
                if (!parseOk) continue;

                double binTarget;
                if (double.TryParse(cols[4].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out binTarget))
                    row.BinaryTarget = binTarget;

                // Multi-class targets (if present, else derive from sales velocity)
                if (cols.Length >= 8)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        double mc;
                        double.TryParse(cols[5 + c].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out mc);
                        row.MultiClassTarget[c] = mc;
                    }
                }
                else
                {
                    row.MultiClassTarget[0] = row.RawInputs[1] < 18 ? 1 : 0;
                    row.MultiClassTarget[1] = (row.RawInputs[1] >= 18 && row.RawInputs[1] < 35) ? 1 : 0;
                    row.MultiClassTarget[2] = row.RawInputs[1] >= 35 ? 1 : 0;
                }

                // Multi-label targets (if present, else derive from business rules)
                if (cols.Length >= 11)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        double ml;
                        double.TryParse(cols[8 + c].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out ml);
                        row.MultiLabelTarget[c] = ml;
                    }
                }
                else
                {
                    row.MultiLabelTarget[0] = (row.RawInputs[0] > 100 && row.RawInputs[1] < 25) ? 1 : 0;
                    row.MultiLabelTarget[1] = row.RawInputs[2] < 60 ? 1 : 0;
                    row.MultiLabelTarget[2] = row.RawInputs[3] >= 32 ? 1 : 0;
                }

                AllRows.Add(row);
            }

            if (AllRows.Count == 0)
                throw new InvalidOperationException("No valid records found in dataset: " + csvPath);

            // Seeded deterministic shuffle to ensure an unbiased, reproducible split
            Random rng = new Random(42);
            List<InventoryRow> shuffled = new List<InventoryRow>(AllRows);
            for (int n = shuffled.Count - 1; n > 0; n--)
            {
                int k = rng.Next(n + 1);
                InventoryRow temp = shuffled[k];
                shuffled[k] = shuffled[n];
                shuffled[n] = temp;
            }

            // Split 80% train, 20% test
            int trainCount = (int)(shuffled.Count * 0.8);
            if (trainCount == 0) trainCount = 1;
            TrainSet = shuffled.GetRange(0, trainCount);
            TestSet = shuffled.GetRange(trainCount, shuffled.Count - trainCount);

            // STRICT ANTI-DATA-LEAKAGE: Fit MinValues and MaxValues EXCLUSIVELY on TrainSet
            for (int c = 0; c < 4; c++)
            {
                MinValues[c] = double.MaxValue;
                MaxValues[c] = double.MinValue;
            }

            foreach (InventoryRow row in TrainSet)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (row.RawInputs[c] < MinValues[c]) MinValues[c] = row.RawInputs[c];
                    if (row.RawInputs[c] > MaxValues[c]) MaxValues[c] = row.RawInputs[c];
                }
            }

            // Prevent division by zero if all values in a feature are identical
            for (int c = 0; c < 4; c++)
            {
                if (MinValues[c] == MaxValues[c])
                    MaxValues[c] = MinValues[c] + 1.0;
            }

            // Transform both TrainSet and TestSet using the fitted training bounds
            foreach (InventoryRow r in TrainSet)
            {
                r.NormInputs = NormalizeInputs(r.RawInputs);
            }
            foreach (InventoryRow r in TestSet)
            {
                r.NormInputs = NormalizeInputs(r.RawInputs);
            }

            // Populate preview table showing explicit train/test partitioning
            int itemNum = 1;
            PopulatePreviewRows(TrainSet, "Train (80%)", ref itemNum);
            PopulatePreviewRows(TestSet, "Test (20%)", ref itemNum);
        }

        private void PopulatePreviewRows(List<InventoryRow> rows, string partition, ref int itemNum)
        {
            foreach (InventoryRow r in rows)
            {
                string tier = r.MultiClassTarget[0] == 1 ? "Slow" : (r.MultiClassTarget[1] == 1 ? "Regular" : "Fast");
                string tags = string.Format("[{0}, {1}, {2}]", (int)r.MultiLabelTarget[0], (int)r.MultiLabelTarget[1], (int)r.MultiLabelTarget[2]);

                DataPreviewTable.Rows.Add(
                    itemNum++,
                    partition,
                    string.Format("{0} ({1:F2})", r.RawInputs[0], r.NormInputs[0]),
                    string.Format("{0} ({1:F2})", r.RawInputs[1], r.NormInputs[1]),
                    string.Format("{0} ({1:F2})", r.RawInputs[2], r.NormInputs[2]),
                    string.Format("{0}% ({1:F2})", r.RawInputs[3], r.NormInputs[3]),
                    (int)r.BinaryTarget,
                    tier,
                    tags
                );
            }
        }

        /// Min-Max normalizes raw input values to [0.0, 1.0] based on training set statistics.
        public double[] NormalizeInputs(double[] raw)
        {
            double[] norm = new double[4];
            for (int c = 0; c < 4; c++)
            {
                double range = MaxValues[c] - MinValues[c];
                norm[c] = range > 0 ? (raw[c] - MinValues[c]) / range : 0.0;
                if (norm[c] < 0.0) norm[c] = 0.0;
                if (norm[c] > 1.0) norm[c] = 1.0;
            }
            return norm;
        }

        // 3. Train All 3 Classification Models & Populate Tables
        public void TrainAndEvaluateAll(int hiddenCount, double lr, int epochs, string logCsvPath)
        {
            MetricsTable.Rows.Clear();
            EpochLogTable.Rows.Clear();

            string dir = Path.GetDirectoryName(logCsvPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (StreamWriter logWriter = new StreamWriter(logCsvPath))
            {
                logWriter.WriteLine("Task,Epoch,Training_MSE_Loss");

                double lastMse;
                BinaryNet = TrainNetwork("Binary (Restock)", 4, hiddenCount, 1, lr, epochs, r => new double[] { r.BinaryTarget }, logWriter, true, out lastMse);
                EvaluateBinary(BinaryNet, string.Format("4 - {0} - 1", hiddenCount), true);

                MultiClassNet = TrainNetwork("Multi-Class (Tier)", 4, hiddenCount, 3, lr, epochs, r => r.MultiClassTarget, logWriter, true, out lastMse);
                EvaluateMultiClass(MultiClassNet, string.Format("4 - {0} - 3", hiddenCount));

                MultiLabelNet = TrainNetwork("Multi-Label (Tags)", 4, hiddenCount, 3, lr, epochs, r => r.MultiLabelTarget, logWriter, true, out lastMse);
                EvaluateMultiLabel(MultiLabelNet, string.Format("4 - {0} - 3", hiddenCount));
            }
        }

        private NeuralNet TrainNetwork(
            string taskName, int inputCount, int hiddenCount, int outputCount,
            double lr, int epochs, Func<InventoryRow, double[]> targetSelector,
            StreamWriter logWriter, bool recordToTable, out double finalMse)
        {
            NeuralNet nn = new NeuralNet(inputCount, hiddenCount, outputCount);
            nn.LRPIN = lr;
            nn.LRPOUT = lr;

            finalMse = 0.0;
            int logInterval = Math.Max(1, epochs / 5);

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                double totalSquaredError = 0.0;

                foreach (InventoryRow row in TrainSet)
                {
                    for (int i = 0; i < inputCount; i++)
                        nn.setInputs(i, row.NormInputs[i]);

                    double[] targets = targetSelector(row);
                    for (int o = 0; o < outputCount; o++)
                        nn.setDesiredOutput(o, targets[o]);

                    nn.learn();

                    for (int o = 0; o < outputCount; o++)
                    {
                        double err = targets[o] - nn.getOuputData(o);
                        totalSquaredError += err * err;
                    }
                }

                finalMse = totalSquaredError / (TrainSet.Count * outputCount);

                if (epoch == 1 || epoch % 100 == 0 || epoch == epochs)
                {
                    if (logWriter != null)
                        logWriter.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2:F4}", taskName, epoch, finalMse));
                }

                if (recordToTable && (epoch == 1 || epoch % logInterval == 0 || epoch == epochs))
                {
                    EpochLogTable.Rows.Add(taskName, epoch, finalMse.ToString("F4"));
                }
            }

            return nn;
        }

        public double EvaluateBinary(NeuralNet nn, string arch, bool addToTable)
        {
            int tp = 0, tn = 0, fp = 0, fn = 0;
            double totalError = 0.0;

            foreach (InventoryRow row in TestSet)
            {
                for (int i = 0; i < 4; i++) nn.setInputs(i, row.NormInputs[i]);
                nn.run();

                double rawPred = nn.getOuputData(0);
                totalError += Math.Pow(row.BinaryTarget - rawPred, 2);

                int pred = rawPred >= 0.5 ? 1 : 0;
                int actual = (int)row.BinaryTarget;

                if (pred == 1 && actual == 1) tp++;
                else if (pred == 0 && actual == 0) tn++;
                else if (pred == 1 && actual == 0) fp++;
                else if (pred == 0 && actual == 1) fn++;
            }

            double mse = totalError / TestSet.Count;
            double accuracy = (double)(tp + tn) / TestSet.Count * 100.0;
            double precision = (tp + fp) > 0 ? (double)tp / (tp + fp) * 100.0 : 0.0;
            double recall = (tp + fn) > 0 ? (double)tp / (tp + fn) * 100.0 : 0.0;

            if (addToTable)
            {
                MetricsTable.Rows.Add(
                    "1. Binary (Restock Alert)",
                    arch,
                    mse.ToString("F4"),
                    string.Format("{0:F2}% ({1}/{2})", accuracy, tp + tn, TestSet.Count),
                    string.Format("{0:F2}%", precision),
                    string.Format("{0:F2}%", recall)
                );
            }
            return accuracy;
        }

        private void EvaluateMultiClass(NeuralNet nn, string arch)
        {
            int correct = 0;
            double totalError = 0.0;
            int[] tp = new int[3], fp = new int[3], fn = new int[3];

            foreach (InventoryRow row in TestSet)
            {
                for (int i = 0; i < 4; i++) nn.setInputs(i, row.NormInputs[i]);
                nn.run();

                int predClass = 0, actualClass = 0;
                double maxProb = -1.0;

                for (int o = 0; o < 3; o++)
                {
                    double outVal = nn.getOuputData(o);
                    totalError += Math.Pow(row.MultiClassTarget[o] - outVal, 2);

                    if (outVal > maxProb) { maxProb = outVal; predClass = o; }
                    if (row.MultiClassTarget[o] == 1.0) actualClass = o;
                }

                if (predClass == actualClass) { correct++; tp[actualClass]++; }
                else { fp[predClass]++; fn[actualClass]++; }
            }

            double macroPrec = 0.0, macroRec = 0.0;
            for (int c = 0; c < 3; c++)
            {
                macroPrec += (tp[c] + fp[c]) > 0 ? (double)tp[c] / (tp[c] + fp[c]) : 0.0;
                macroRec += (tp[c] + fn[c]) > 0 ? (double)tp[c] / (tp[c] + fn[c]) : 0.0;
            }
            macroPrec = (macroPrec / 3.0) * 100.0;
            macroRec = (macroRec / 3.0) * 100.0;

            double mse = totalError / (TestSet.Count * 3);
            double accuracy = (double)correct / TestSet.Count * 100.0;

            MetricsTable.Rows.Add(
                "2. Multi-Class (Velocity Tier)",
                arch,
                mse.ToString("F4"),
                string.Format("{0:F2}% ({1}/{2})", accuracy, correct, TestSet.Count),
                string.Format("{0:F2}% (Macro)", macroPrec),
                string.Format("{0:F2}% (Macro)", macroRec)
            );
        }

        private void EvaluateMultiLabel(NeuralNet nn, string arch)
        {
            int exactMatch = 0, totalLabelCorrect = 0;
            int tp = 0, fp = 0, fn = 0;
            double totalError = 0.0;

            foreach (InventoryRow row in TestSet)
            {
                for (int i = 0; i < 4; i++) nn.setInputs(i, row.NormInputs[i]);
                nn.run();

                bool allThreeMatch = true;
                for (int o = 0; o < 3; o++)
                {
                    double outVal = nn.getOuputData(o);
                    totalError += Math.Pow(row.MultiLabelTarget[o] - outVal, 2);

                    int pred = outVal >= 0.5 ? 1 : 0;
                    int actual = (int)row.MultiLabelTarget[o];

                    if (pred == actual) totalLabelCorrect++;
                    else allThreeMatch = false;

                    if (pred == 1 && actual == 1) tp++;
                    else if (pred == 1 && actual == 0) fp++;
                    else if (pred == 0 && actual == 1) fn++;
                }
                if (allThreeMatch) exactMatch++;
            }

            double mse = totalError / (TestSet.Count * 3);
            double labelAcc = (double)totalLabelCorrect / (TestSet.Count * 3) * 100.0;
            double microPrec = (tp + fp) > 0 ? (double)tp / (tp + fp) * 100.0 : 0.0;
            double microRec = (tp + fn) > 0 ? (double)tp / (tp + fn) * 100.0 : 0.0;

            MetricsTable.Rows.Add(
                "3. Multi-Label (Store Tags)",
                arch,
                mse.ToString("F4"),
                string.Format("{0:F2}% (Label) / {1}% (Exact)", labelAcc, (int)((double)exactMatch / TestSet.Count * 100)),
                string.Format("{0:F2}% (Micro)", microPrec),
                string.Format("{0:F2}% (Micro)", microRec)
            );
        }

        // 4. Hyperparameter Tuning Table
        public void RunHyperparameterTuning()
        {
            TuningTable.Rows.Clear();

            int[] hiddens = { 4, 8, 16 };
            double[] lrs = { 0.05, 0.15, 0.40 };
            int[] epochList = { 300, 1000, 1500 };
            string[] notes = {
                "Too few epochs & slow learning rate (Underfitting)",
                "Best balance of convergence speed & accuracy (Optimal)",
                "Larger network & aggressive learning rate"
            };

            for (int i = 0; i < 3; i++)
            {
                double finalMse;
                NeuralNet net = TrainNetwork("Tune", 4, hiddens[i], 1, lrs[i], epochList[i], r => new double[] { r.BinaryTarget }, null, false, out finalMse);
                double acc = EvaluateBinary(net, "", false);

                TuningTable.Rows.Add(
                    "Trial " + (i + 1),
                    hiddens[i],
                    lrs[i].ToString("F2"),
                    epochList[i],
                    finalMse.ToString("F4"),
                    string.Format("{0:F2}%", acc),
                    notes[i]
                );
            }
        }

        // 5. Single Item Live Prediction (Returns 3 separate strings)
        public void PredictSingleItem(double stock, double sales, double expiry, double margin,
            out string binaryResult, out string multiClassResult, out string multiLabelResult)
        {
            double[] norm = NormalizeInputs(new double[] { stock, sales, expiry, margin });

            for (int i = 0; i < 4; i++) BinaryNet.setInputs(i, norm[i]);
            BinaryNet.run();
            double binProb = BinaryNet.getOuputData(0);
            binaryResult = binProb >= 0.5
                ? string.Format("Restock Needed ({0:F1}%)", binProb * 100)
                : string.Format("Safe Stock ({0:F1}%)", (1.0 - binProb) * 100);

            for (int i = 0; i < 4; i++) MultiClassNet.setInputs(i, norm[i]);
            MultiClassNet.run();
            string[] tierNames = { "Slow-Moving", "Regular", "Fast-Moving" };
            int bestTier = 0;
            double maxProb = -1.0;
            for (int o = 0; o < 3; o++)
            {
                if (MultiClassNet.getOuputData(o) > maxProb)
                {
                    maxProb = MultiClassNet.getOuputData(o);
                    bestTier = o;
                }
            }
            multiClassResult = string.Format("{0} ({1:F1}%)", tierNames[bestTier], maxProb * 100);

            for (int i = 0; i < 4; i++) MultiLabelNet.setInputs(i, norm[i]);
            MultiLabelNet.run();
            List<string> activeTags = new List<string>();
            if (MultiLabelNet.getOuputData(0) >= 0.5) activeTags.Add("Promo Discount");
            if (MultiLabelNet.getOuputData(1) >= 0.5) activeTags.Add("Expiry Risk");
            if (MultiLabelNet.getOuputData(2) >= 0.5) activeTags.Add("High Profit");
            multiLabelResult = activeTags.Count > 0 ? string.Join(", ", activeTags) : "None (Standard)";
        }
    }
}
