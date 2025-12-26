"""
Registry for Demand Forecasting Strategies
"""
from api.core.registry import StrategyRegistry
from api.strategies.demand.lightgbm_strategy import LightGBMStrategy

# Create registry instance
demand_registry = StrategyRegistry()

# Register strategies
demand_registry.register("lightgbm", LightGBMStrategy())

# Set default
demand_registry.set_default("lightgbm")

__all__ = ["demand_registry"]

