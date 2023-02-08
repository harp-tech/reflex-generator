from __future__ import annotations
from typing import Optional, List
import attr
from attr import define

@define
class IOElement:
    name: str
    port: str
    pin: int
    direction: str
    useInput: Optional[bool] = None
    pull: Optional[str] = None
    sense: Optional[str] = None
    interruptPriority: Optional[str] = None
    interruptNumber: Optional[int] = None
    out: Optional[str] = None
    outDefault: Optional[bool] = None
    outInvert: Optional[bool] = None
    description: Optional[str] = None

    def to_dict(self):
        return attr.asdict(self)


class IOElementCollection:
    def __init__(
        self,
        element_array: Optional[List[IOElement]],
        ) -> None:

        self.elements = []
        if element_array:
            self.from_array(element_array)

    def __iter__(self):
        return iter(self.elements)

    def from_array(self, arr: List[IOElement]) -> None:
        if len(arr) < 1:
            raise ValueError("List can't be empty!")

        for element in arr:
            self.append(element)

    def append(self, element: IOElement) -> None:
        self.elements.append(element)

    def insert(self, idx: int, element: IOElement) -> None:
        self.elements.insert(idx, element)

    def pop(self, idx: Optional[int]) -> None:
        self.elements.pop(idx)

    def __getitem__(self, index):
        return self.elements[index]