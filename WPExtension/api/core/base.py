"""
Base Strategy Interface
"""
from abc import ABC, abstractmethod
from typing import Any, Dict


class BaseStrategy(ABC):
    """Base class for all prediction strategies"""
    
    def __init__(self):
        self._loaded = False
    
    @abstractmethod
    def load(self) -> None:
        """Load model and dependencies"""
        pass
    
    @abstractmethod
    def predict(self, features: Dict[str, Any]) -> float:
        """Make prediction from features
        
        Args:
            features: Dictionary of feature names to values
            
        Returns:
            Predicted value
        """
        pass
    
    @abstractmethod
    def get_name(self) -> str:
        """Return strategy name"""
        pass
    
    def is_loaded(self) -> bool:
        """Check if strategy is loaded"""
        return self._loaded
    
    def ensure_loaded(self) -> None:
        """Ensure strategy is loaded, load if not"""
        if not self._loaded:
            self.load()
