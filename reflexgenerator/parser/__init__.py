import pandas as pd

from pathlib import Path

class ElementParser():
    def __init__(
        self,
        target: str | Path) -> pd.DataFrame:

        if isinstance(target, Path):
            self.table = pd.read_csv(target)
        if isinstance(target, str):
            self.table = pd.read_table(target)

