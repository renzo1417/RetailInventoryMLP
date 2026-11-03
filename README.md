# Real-World Retail Inventory Prediction with a Custom Multi-Layer Perceptron (C#)

An end-to-end intelligent retail inventory management and classification system powered by a custom **3-layer Multi-Layer Perceptron (MLP)** built from scratch in C# using backpropagation.

Developed for the **Real-World Prediction with a Custom Multi-Layer Perceptron in C#** course activity.

---

## 📌 Executive Overview

Retail stores often struggle with balancing stock availability against working capital and spoilage risk. Stocking too little leads to lost sales and dissatisfied customers, while overstocking leads to tied-up cash and expiration waste.

This project implements a multi-task neural network classification engine in C# that processes inventory metrics (**Stock Quantity, Daily Sales Velocity, Days to Expiry, Profit Margin %**) to perform three simultaneous machine learning tasks:
1. **Binary Classification**: Predicts `RestockAlert` (0 = Safe Stock, 1 = Urgent Restock Needed).
2. **Multi-Class Classification**: Classifies items into turnover tiers (`Slow-Moving`, `Regular`, `Fast-Moving`).
3. **Multi-Label Classification**: Simultaneously flags relevant store strategy tags (`[Promo Discount, Expiry Risk, High Profit]`).

---

## 🧠 Neural Network Engine & Architecture

The system strictly adheres to the course constraint:
> *"ONLY use the given libraries from C# (input, hidden, output) backneural classes."*

* **Input Layer (`INeuron.cs`)**: Accepts 4 normalized continuous features.
* **Hidden Layer (`HNeuron.cs`)**: Fully connected hidden layer with configurable hidden neurons (default: 8), Sigmoid activation, and backpropagated error gradients.
* **Output Layer (`OuputN.cs`)**: Outputs task-specific activations:
  * Binary Task: $4 \to 8 \to 1$ network (Sigmoid threshold $\ge 0.5$).
  * Multi-Class Task: $4 \to 8 \to 3$ network (One-Hot target with $\text{argmax}$ selection).
  * Multi-Label Task: $4 \to 8 \to 3$ network (Independent Sigmoid thresholding per tag).
* **Backpropagation Engine (`BackNeural.cs`)**: Implements gradient descent weight and bias updates:
  $$\Delta w_{ij} = \eta \cdot \delta_j \cdot a_i$$
  $$\delta_k^{out} = (y_k - \hat{y}_k) \cdot \hat{y}_k(1 - \hat{y}_k)$$
  $$\delta_j^{hidden} = \left(\sum_k \delta_k^{out} w_{jk}\right) \cdot a_j(1 - a_j)$$

---

## 🛠️ Data Preprocessing & Anti-Leakage Pipeline

* **Data Parsing**: Cleanly reads CSV datasets with support for whitespace trimming and missing-value resilience.
* **Deterministic Shuffling**: Applies a Fisher–Yates shuffle with fixed seed `Random(42)` to eliminate row-ordering bias before partitioning.
* **80 / 20 Train-Test Split**: Divides records into 160 training samples and 40 testing samples.
* **Anti-Data-Leakage Min-Max Scaling**:
  $$\text{Norm}(x) = \frac{x - \min(X_{\text{train}})}{\max(X_{\text{train}}) - \min(X_{\text{train}})}$$
  **Crucial Invariant**: Minima and maxima are fitted **strictly on the training partition**. The test partition is transformed using the training parameters, ensuring zero statistical leakage.

---

## 🖥️ System Architecture & Modularity

The codebase follows clean C# coding standards with distinct separation of concerns:

```
RetailInventoryMLP/
├── BackNeural.cs          # Core NeuralNet engine (feedforward, learn loop, I/O)
├── InputN.cs              # Input layer neuron definition
├── HiddenN.cs             # Hidden layer neuron with error backpropagation
├── OuputN.cs              # Output layer neuron definition
├── InventoryEngine.cs     # Business logic, anti-leakage pipeline, training & metrics
├── Form1.cs               # Asynchronous UI controller & event handlers
├── Form1.Designer.cs      # Modern Windows Forms layout & DataGridView bindings
├── Program.cs             # Application entry point
├── inventory_data.csv     # Sample 200-item retail dataset
└── training_log.csv       # Training loss per epoch export
```

---

## 🚀 How to Build & Run

### Prerequisites
* **Windows OS**
* **.NET Framework 4.7.2** or higher
* **Visual Studio 2022** (or .NET SDK CLI with MSBuild)

### Option 1: Visual Studio 2022 (Recommended)
1. Double-click `RetailInventoryMLP.slnx` or open the project folder in Visual Studio.
2. Set configuration to `Debug` or `Release` (`Any CPU`).
3. Press `F5` (or click **Start**) to build and run.

### Option 2: Command Line (.NET SDK)
From the repository root:
```powershell
dotnet build RetailInventoryMLP\RetailInventoryMLP.csproj
.\RetailInventoryMLP\bin\Debug\RetailInventoryMLP.exe
```

---

## 📖 Application User Guide

### 1. Control Panel (Left Sidebar)
* **Load Default CSV**: Loads the built-in 200-item dataset (or generates it deterministically if missing).
* **Browse Custom CSV**: Opens an `OpenFileDialog` allowing you to load and test any independent external CSV.
* **Hidden Neurons / Learning Rate / Epochs**: Configurable hyperparameter inputs (defaults: `8`, `0.15`, `1000`).
* **Train & Evaluate Models**: Launches asynchronous background training (`Task.Run`) for all 3 classification networks. The UI remains responsive with a wait cursor and live status messages.
* **Run Hyperparameter Tests**: Executes 3 structured experimental trials and populates comparative performance metrics.
* **Test Single Item Prediction**: Enter raw stock, sales, expiry, and profit margin values to receive instant real-time predictions across all 3 models.

### 2. Results & Analysis Viewer (Right DataGridView)
Switch views seamlessly using the radio buttons above the table:
* 🔘 **1. Evaluation Metrics**: Displays Test Loss (MSE), Accuracy %, Precision, and Recall for each task.
* 🔘 **2. Epoch Loss Log**: Shows MSE loss decreasing across epochs.
* 🔘 **3. Hyperparameter Tests**: Shows trial comparison across different architectures and rates.
* 🔘 **4. Normalized Dataset**: Shows raw vs. normalized values and partition tags (`Train (80%)` vs `Test (20%)`).

---

## 📂 Documentation & Deliverables Mapping

| Required Deliverable | Location in Repository |
| :--- | :--- |
| **Source Code via GitHub** | Git repository containing all C# source files |
| **Training Log / Output** | [`RetailInventoryMLP/bin/Debug/training_log.csv`](file:///d:/Source/Repos/RetailInventoryMLP/RetailInventoryMLP/bin/Debug/training_log.csv) and UI View 2 |
| **Data Description** | [`DATA_DESCRIPTION.md`](file:///d:/Source/Repos/RetailInventoryMLP/DATA_DESCRIPTION.md) |
| **Hyperparameter & Evaluation Analysis** | [`ANALYSIS_REPORT.md`](file:///d:/Source/Repos/RetailInventoryMLP/ANALYSIS_REPORT.md) |
