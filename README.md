# Retail Inventory Prediction (Custom MLP in C#)

A Windows Forms application built in C# that uses a custom 3-layer Multi-Layer Perceptron (MLP) with backpropagation to predict retail inventory decisions.

---

## What It Does

The app takes 4 basic inventory metrics for a product:
1. **Stock Quantity** (current units on hand)
2. **Daily Sales** (average units sold per day)
3. **Days to Expiry** (shelf life remaining)
4. **Profit Margin %** (margin percentage)

And predicts three classification tasks:
* **Binary Classification**: Whether the item needs urgent restocking (`Restock Needed` vs. `Safe Stock`).
* **Multi-Class Classification**: Sales velocity tier (`Slow-Moving`, `Regular`, or `Fast-Moving`).
* **Multi-Label Classification**: Store strategy tags (`Promo Discount`, `Expiry Risk`, `High Profit`).

---

## Project Structure

* **`BackNeural.cs`**: The core `NeuralNet` class handling the forward pass and backpropagation training loop.
* **`InputN.cs`**, **`HiddenN.cs`**, **`OuputN.cs`**: Neuron classes representing the input, hidden, and output layers.
* **`InventoryEngine.cs`**: Contains data loading, Min-Max scaling (fitted on train set to prevent data leakage), 80/20 train/test splitting, model training, evaluation metrics, and hyperparameter tuning.
* **`Form1.cs` / `Form1.Designer.cs`**: The WinForms GUI with asynchronous background training (`Task.Run`) so the UI stays responsive while training.
* **`inventory_data.csv`**: The 200-row sample inventory dataset.
* **`training_log.csv`**: Exported training loss logged every 100 epochs.
* **`DATA_DESCRIPTION.md`**: Breakdown of dataset features and target definitions.
* **`ANALYSIS_REPORT.md`**: Hyperparameter tuning results and evaluation metric analysis.

---

## How to Run

### Requirements
* Windows
* Visual Studio 2022 (with .NET desktop development) or .NET Framework 4.7.2+

### In Visual Studio
1. Open `RetailInventoryMLP.slnx`.
2. Press **F5** (or click **Start**).

### From Command Line
```powershell
dotnet build RetailInventoryMLP\RetailInventoryMLP.csproj
.\RetailInventoryMLP\bin\Debug\RetailInventoryMLP.exe
```

---

## Using the App

1. **Load Data**: Click **Load Default CSV** to load `inventory_data.csv`, or click **Browse Custom CSV** to pick your own file.
2. **Train**: Set Hidden Neurons, Learning Rate, and Epochs (defaults are 8, 0.15, 1000), then click **Train & Evaluate Models**.
3. **View Results**: Use the radio buttons on top of the grid to switch between:
   * **Evaluation Metrics**: Test MSE, Accuracy, Precision, and Recall.
   * **Epoch Loss Log**: Training loss decreasing across epochs.
   * **Hyperparameter Tests**: Comparison across different tuning trials.
   * **Normalized Dataset**: The raw and normalized dataset showing train/test partitions.
4. **Single Item Test**: Enter stock, sales, expiry, and profit margin in the bottom-left inputs and click **Test Single Item Prediction** for live results.
