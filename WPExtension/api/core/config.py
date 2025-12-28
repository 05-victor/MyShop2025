"""
Configuration Management
"""
import os
from pathlib import Path
from typing import Dict


# Base paths
BASE_DIR = Path(__file__).parent.parent.parent
DEMAND_FORECAST_DIR = BASE_DIR / "Demand Forecasting"
PRICE_FORECAST_DIR = BASE_DIR / "Price Forecasting"
PF_DATASET_DIR = BASE_DIR / "PF dataset"

# Model paths
DEMAND_MODEL_DIR = DEMAND_FORECAST_DIR / "weight"
DEMAND_ENCODER_DIR = DEMAND_FORECAST_DIR / "encoders"
DEMAND_CONFIG_PATH = DEMAND_MODEL_DIR / "config.json"

PRICE_MODEL_DIR = PRICE_FORECAST_DIR / "model"
PRICE_LINEAR_MODEL = PRICE_MODEL_DIR / "linear_regressor.pkl"
PRICE_DNN_JSON = PRICE_MODEL_DIR / "dnn_regressor.json"
PRICE_DNN_WEIGHTS = PRICE_MODEL_DIR / "dnn_regressor.weights.h5"

# Dataset paths
PF_TRAIN_CSV = PF_DATASET_DIR / "train.csv"
PF_STORES_CSV = PF_DATASET_DIR / "stores.csv"
PF_FEATURES_CSV = PF_DATASET_DIR / "features.csv"

# Default strategies
DEFAULT_DEMAND_STRATEGY = "lightgbm"
DEFAULT_PRICE_STRATEGY = "linear"


def get_config() -> Dict:
    """Get configuration dictionary"""
    return {
        "demand": {
            "model_dir": str(DEMAND_MODEL_DIR),
            "encoder_dir": str(DEMAND_ENCODER_DIR),
            "config_path": str(DEMAND_CONFIG_PATH),
            "default_strategy": DEFAULT_DEMAND_STRATEGY,
            "n_folds": 10
        },
        "price": {
            "model_dir": str(PRICE_MODEL_DIR),
            "linear_model": str(PRICE_LINEAR_MODEL),
            "dnn_json": str(PRICE_DNN_JSON),
            "dnn_weights": str(PRICE_DNN_WEIGHTS),
            "default_strategy": DEFAULT_PRICE_STRATEGY,
            "datasets": {
                "train": str(PF_TRAIN_CSV),
                "stores": str(PF_STORES_CSV),
                "features": str(PF_FEATURES_CSV)
            }
        }
    }


def validate_paths() -> Dict[str, bool]:
    """Validate that required paths exist"""
    config = get_config()
    results = {}
    
    # Demand paths
    demand_config = config["demand"]
    results["demand_model_dir"] = Path(demand_config["model_dir"]).exists()
    results["demand_encoder_dir"] = Path(demand_config["encoder_dir"]).exists()
    results["demand_config"] = Path(demand_config["config_path"]).exists()
    
    # Price paths
    price_config = config["price"]
    results["price_model_dir"] = Path(price_config["model_dir"]).exists()
    results["price_linear_model"] = Path(price_config["linear_model"]).exists()
    
    # Dataset paths
    for key, path in price_config["datasets"].items():
        results[f"price_dataset_{key}"] = Path(path).exists()
    
    return results
