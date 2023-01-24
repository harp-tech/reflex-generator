from enum import Enum

HarpDataType = Enum("HarpDataType",[
    "NONE",
    "U8", "U16", "U32", "U64",
    "S8", "S16", "S32", "S64",
    "float"
    ])


class HarpElement:
    "Class that represents a single element (e.g. register or mask)"
    def __init__(self,
    name: str,
    ) -> None:
        self.name = name