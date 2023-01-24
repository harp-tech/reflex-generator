import pandas as pd
import sources

from typing import Optional


class Register(sources.HarpElement):

    def __init__(
        self,
        name: str,
        address: int,
        dtype: sources.HarpDataType,
        is_event: bool = False,
        mask_family: Optional[str] = None,
        description: Optional[str] = None,
        converter: Optional[str] = None,
        enable_generator: bool = True
    ) -> None:

        super().__init__(name=name)
        self.address = address
        self.dtype = dtype
        self.is_event = is_event
        self.mask_family = mask_family
        self.description = description
        self.converter = converter
        self.enable_generator = enable_generator
