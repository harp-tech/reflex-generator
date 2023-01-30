from pathlib import Path
import pandas as pd
from typing import Optional, Dict


class ElementTable():
    def __init__(
        self,
        target: str | Path,
        reader_kwargs: Dict[str, any] = {}) -> None:

        self._target = target
        self._raw_table = self._load_table(reader_kwargs)

    # Make target read-only
    @property
    def target(self):
        return self._target

    @property
    def raw_table(self):
        return self._raw_table

    def _load_table(self, reader_kwargs) -> pd.DataFrame:
        if isinstance(self.target, Path):
            return pd.read_csv(self.target, **reader_kwargs)
        if isinstance(self.target, str):
            return pd.read_table(self.target, **reader_kwargs)


