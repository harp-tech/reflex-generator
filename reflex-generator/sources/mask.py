import pandas as pd
import sources

from typing import Optional


class Mask(sources.HarpElement):

    def __init__(
        self,
        name: str,
        mask: str | int,
        dtype: sources.HarpDataType,
        mask_family: Optional[str] = None,
        description: Optional[str] = None,
        converter: Optional[str] = None,
        enable_generator: bool = True
    ) -> None:

        super().__init__(name=name)
        self.mask = mask
        self.dtype = dtype
        self.mask_family = mask_family
        self.description = description
        self.converter = converter
        self.enable_generator = enable_generator
