"""
Base Registry Class for Strategy Pattern
"""
from typing import Dict, List, Optional
from api.core.base import BaseStrategy


class StrategyRegistry:
    """Registry pattern để quản lý strategies"""
    
    def __init__(self):
        self._strategies: Dict[str, BaseStrategy] = {}
        self._default_name: Optional[str] = None
    
    def register(self, name: str, strategy: BaseStrategy) -> None:
        """Đăng ký strategy
        
        Args:
            name: Strategy name
            strategy: Strategy instance
        """
        self._strategies[name] = strategy
    
    def get(self, name: Optional[str] = None) -> BaseStrategy:
        """Lấy strategy theo tên, hoặc default nếu không chỉ định
        
        Args:
            name: Strategy name, None để dùng default
            
        Returns:
            Strategy instance
            
        Raises:
            ValueError: Nếu strategy không tồn tại
        """
        if name is None:
            name = self._default_name
        
        if name is None:
            raise ValueError("No default strategy set and no strategy name provided")
        
        if name not in self._strategies:
            raise ValueError(f"Strategy '{name}' not found. Available: {self.list_all()}")
        
        return self._strategies[name]
    
    def list_all(self) -> List[str]:
        """Liệt kê tất cả strategies
        
        Returns:
            List of strategy names
        """
        return list(self._strategies.keys())
    
    def set_default(self, name: str) -> None:
        """Set default strategy
        
        Args:
            name: Strategy name
            
        Raises:
            ValueError: Nếu strategy không tồn tại
        """
        if name not in self._strategies:
            raise ValueError(f"Strategy '{name}' not found. Available: {self.list_all()}")
        self._default_name = name
    
    def get_default_name(self) -> Optional[str]:
        """Get default strategy name"""
        return self._default_name
    
    def load_all(self) -> None:
        """Load all registered strategies (lazy loading)"""
        for strategy in self._strategies.values():
            if not strategy.is_loaded():
                strategy.load()
