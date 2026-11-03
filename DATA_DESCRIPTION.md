# Dataset Description & Data Pipeline

**Course Activity**: Real-World Prediction with a Custom Multi-Layer Perceptron in C#  
**Deliverable**: Deliverable 3 — Data Description & Preprocessing Pipeline  
**Dataset File**: `inventory_data.csv` (200 records $\times$ 11 columns)

---

## 1. Domain Background & Business Problem

In fast-paced retail environments (supermarkets, convenience stores, and electronics retail), inventory management represents a critical trade-off between operational cash flow and customer satisfaction:
* **Understocking (Stockouts)** results in immediate lost revenue, customer churn, and compromised service-level agreements.
* **Overstocking** consumes expensive warehouse space, increases financing holding costs, and leads to severe write-offs due to product expiration or obsolescence.

To solve this, our multi-task neural network engine processes 4 operational inventory signals to output actionable restocking alerts, inventory turnover classifications, and store merchandising strategies.

---

## 2. Dataset Schema & Feature Dictionary

The dataset consists of **200 item records** with continuous feature inputs and three distinct classification target formats.

### 2.1 Input Features (Predictors)

| Column Name | Data Type | Units / Scale | Range | Business Description |
| :--- | :---: | :---: | :---: | :--- |
| `StockQty` | Integer / Continuous | Units | $5 \text{ to } 200$ | Current physical inventory count remaining in the store or warehouse. |
| `DailySales` | Integer / Continuous | Units / Day | $1 \text{ to } 50$ | Average customer purchase velocity per day over the trailing 14-day window. |
| `DaysToExpiry` | Integer / Continuous | Days | $3 \text{ to } 180$ | Remaining shelf life until the batch expires or reaches obsolescence. |
| `ProfitMargin` | Integer / Continuous | Percentage (%) | $5\% \text{ to } 60\%$ | Net gross profit margin earned per unit sold. |

---

### 2.2 Target Variables & Classification Tracks

The dataset incorporates **all three classification tracks** supported by the rubric:

#### A. Track 1: Binary Classification — `RestockAlert`
* **Column**: `RestockAlert` (Column index 4)
* **Classes**:
  * `0`: **Safe Stock** — Inventory level is sufficient for anticipated demand.
  * `1`: **Urgent Reorder Required** — Stock has fallen below the safety buffer or sales velocity threatens an imminent stockout.
* **Domain Logic**:
  $$\text{Restock} = 1 \iff (\text{StockQty} < 35) \lor (\text{StockQty} < 75 \land \text{DailySales} > 20)$$

#### B. Track 2: Multi-Class Classification — Inventory Velocity Tier
* **Columns**: `Tier_Slow`, `Tier_Regular`, `Tier_Fast` (Columns 5, 6, 7)
* **Format**: 3-dimensional One-Hot Vector ($[1,0,0]$, $[0,1,0]$, $[0,0,1]$).
* **Classes**:
  * **Slow-Moving Tier** (`sales < 18`): Candidates for clearance or reduced shelf space.
  * **Regular Tier** (`18 <= sales < 35`): Standard reorder cycle items.
  * **Fast-Moving Tier** (`sales >= 35`): High-turnover hero products requiring priority supplier contracts.

#### C. Track 3: Multi-Label Classification — Merchandising Strategy Tags
* **Columns**: `Tag_PromoDiscount`, `Tag_ExpiryRisk`, `Tag_HighProfit` (Columns 8, 9, 10)
* **Format**: Multi-hot binary vector; an item may have zero, one, two, or all three tags simultaneously.
* **Tags**:
  1. `Tag_PromoDiscount` ($\text{StockQty} > 100 \land \text{DailySales} < 25$): Flagged for promotional price markdown to free up tied-up capital.
  2. `Tag_ExpiryRisk` ($\text{DaysToExpiry} < 60$): Flagged for immediate FIFO rotation or bundle discounts to avoid spoilage loss.
  3. `Tag_HighProfit` ($\text{ProfitMargin} \ge 32\%$): Flagged for prominent aisle positioning to maximize basket profitability.

---

## 3. Data Preprocessing & Anti-Leakage Scaling Pipeline

### 3.1 Deterministic Shuffling
Prior to splitting, the 200 items are shuffled using an in-place Fisher–Yates algorithm with a fixed pseudo-random seed (`Random(42)`). This ensures:
1. Complete reproducibility across multiple application launches.
2. Removal of any sequential clustering or data ordering artifacts.

### 3.2 80 / 20 Train-Test Partitioning
* **Training Partition**: $80\%$ (160 items)
* **Testing Partition**: $20\%$ (40 items)

### 3.3 Strict Anti-Data-Leakage Min-Max Normalization
In machine learning, applying normalizers across the full dataset prior to splitting leaks the test set distribution into the training pipeline. To adhere to strict academic standards:
1. **Fit**: The minimum ($\min_{\text{train}}$) and maximum ($\max_{\text{train}}$) are computed **exclusively on the 160 training rows**.
2. **Transform**: Both training and test vectors are mapped to $[0.0, 1.0]$ using the training parameters:
   $$x_{\text{norm}} = \text{Clamp}_{[0, 1]}\left(\frac{x - \min(X_{\text{train}})}{\max(X_{\text{train}}) - \min(X_{\text{train}})}\right)$$
3. **Inference**: Any single-item live prediction entered in the UI is transformed using these exact training parameters.

---

## 4. Sample Raw vs. Normalized Data Records

Below is an extract from the training partition demonstrating the normalized feature inputs fed into the neural network input neurons:

| Item # | Partition | StockQty (Raw / Norm) | DailySales (Raw / Norm) | DaysToExpiry (Raw / Norm) | ProfitMargin (Raw / Norm) | Restock Target | Tier Target | Strategy Tags |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| 1 | Train (80%) | 135 (0.67) | 8 (0.14) | 25 (0.11) | 34% (0.53) | 0 (Safe) | Slow | `[1, 1, 1]` |
| 2 | Train (80%) | 38 (0.17) | 14 (0.27) | 131 (0.72) | 33% (0.51) | 0 (Safe) | Slow | `[0, 0, 1]` |
| 3 | Train (80%) | 39 (0.17) | 39 (0.78) | 44 (0.22) | 19% (0.25) | 1 (Restock) | Fast | `[0, 1, 0]` |
| 4 | Train (80%) | 104 (0.51) | 17 (0.33) | 70 (0.37) | 19% (0.25) | 0 (Safe) | Slow | `[1, 0, 0]` |
| 5 | Train (80%) | 14 (0.05) | 40 (0.80) | 114 (0.62) | 11% (0.11) | 1 (Restock) | Fast | `[0, 0, 0]` |

---

## 5. Support for External Independent Datasets

In accordance with **Objective 1** (*"Apply your custom C# neural network framework to an independent dataset"*), the application features a **"Browse Custom CSV"** dialog. 

When loading an external CSV file:
* The engine accepts arbitrary row counts.
* Trailing whitespaces and blank rows are safely discarded.
* If optional multi-class or multi-label columns are omitted in an external file, the engine automatically derives them from the 4 primary continuous features using standard retail classification heuristics.
