"""
Registry for Price Forecasting strategies (linear only)
"""
from api.core.registry import StrategyRegistry
from api.strategies.price.linear_strategy import LinearStrategy

# Create registry instance
price_registry = StrategyRegistry()

# Register only the linear strategy by design
price_registry.register("linear", LinearStrategy())

# Set default strategy
price_registry.set_default("linear")

