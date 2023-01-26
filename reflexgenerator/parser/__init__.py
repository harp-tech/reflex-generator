from pathlib import Path

import pandas as pd

_col2mask = {
    "Name": "name",
    "MaskFamily": "mask_family",
    "Mask": "mask",
    "Format": "dtype",
    "Description": "description",
    "Converter": "converter",
    "AutoInterface": "enable_generator"
}

_col2register = {
    "Name": "name",
    "Address": "address",
    "Format": "dtype",
    "PayloadFormat": "array_spec",
    "Type": "is_event",
    "Description": "description",
    "MaskFamily": "mask_family",
    "Converter": "converter",
    "AutoInterface": "enable_generator"
}

_mask2col = dict([reversed(i) for i in _col2mask.items()])

_register2col = dict([reversed(i) for i in _col2register.items()])


class ElementParser():
    def __init__(
        self,
        target: str | Path
        ) -> pd.DataFrame:

        if isinstance(target, Path):
            self.table = pd.read_csv(target)
        if isinstance(target, str):
            self.table = pd.read_table(target)

