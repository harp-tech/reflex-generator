from __future__ import annotations
import attr

from attr import define
from typing import Optional, List, Dict
from enum import Enum

from reflexgenerator.generator.markdown import AnchorReference


_MASKS = {}

# Type shorthands

PayloadType = Enum("PayloadType", [
    "U8", "U16", "U32", "U64",
    "S8", "S16", "S32", "S64",
    "Float"])


def _payloadType_converter(value: PayloadType | str) -> PayloadType:
    if isinstance(value, str):
        return PayloadType[value]
    if isinstance(value, PayloadType):
        return value
    raise TypeError("Must be PayloadType or str.")


RegisterType = Enum("RegisterType", [
    "NONE", "Command", "Event"
])


def _registerType_converter(value: RegisterType | str) -> RegisterType:
    if isinstance(value, str):
        return RegisterType[value]
    if isinstance(value, RegisterType):
        return value
    raise TypeError("Must be RegisterType or str.")


VisibilityType = Enum("VisibilityType", [
    "Public", "Private"
])


def _visibilityType_converter(value: VisibilityType | str) -> VisibilityType:
    if isinstance(value, str):
        return VisibilityType[value]
    if isinstance(value, VisibilityType):
        return value
    raise TypeError("Must be VisibilityType or str.")


@define
class Bit:
    name: attr.ib(default=None, type=Optional[str], converter=str)
    value: attr.ub(default=None, type=Optional[int], converter=hex)

    @classmethod
    def from_dict(self, value_dict: Dict[str, int]):
        assert len(value_dict) == 2
        return Bit(value_dict[0], value_dict[1])


def _make_bit_array(value: Optional[Dict[str, int]]) -> Optional[List[Bit]]:
    if value is None:
        return None
    if isinstance(value, dict):
        return [Bit.from_dict(bit) for bit in value.items()]


@define
class BitMask:
    name = attr.ib(type=str, converter=str)
    payloadType = attr.ib(type=PayloadType | str,
                          converter=_payloadType_converter)
    description = attr.ib(default=None,
                          type=Optional[str], converter=str)
    bits = attr.ib(default=None,
                   type=Optional[List[Bit]], converter=_make_bit_array)
    _uref = attr.ib(init=False)

    def __attrs_post_init__(self):
        _MASKS.update({self.name: self})
        self._uref = AnchorReference(self.name, self.name, self)

    def to_dict(self) -> Dict[str, any]:
        return attr.asdict(self)

    def render_uref(self, label: Optional[str] = None) -> str:
        return self._uref.render_reference(label)

    def render_pointer(self, label: Optional[str] = None) -> str:
        return self._uref.render_pointer(label)


def get_bit_mask(value: Optional[str]) -> Optional[BitMask]:
    if value is None:
        return None
    else:
        if value in list(_MASKS.keys()):
            return _MASKS[value]
        else:
            raise KeyError("Specified mask has not been defined.")

@define
class Register:
    name = attr.ib(type=str)
    address = attr.ib(type=int, converter=int)
    payloadType = attr.ib(type=PayloadType | str,
                          converter=_payloadType_converter)
    alias = attr.ib(default=None, type=Optional[str])
    arrayType = attr.ib(default=1, type=(str | List[str] | int))
    registerType = attr.ib(default=RegisterType.NONE,
                           type=str, converter=_registerType_converter)
    maskType = attr.ib(default=None,
                       type=Optional[List[BitMask]], converter=get_bit_mask)
    description = attr.ib(default=None, type=Optional[str], converter=str)
    converter = attr.ib(default=None, type=Optional[bool])
    visibility = attr.ib(default=VisibilityType.Public,
                         type=str, converter=_visibilityType_converter)
    group = attr.ib(default=None, type=Optional[str], converter=str)
    _uref = attr.ib(init=False)

    def __attrs_post_init__(self):
        self._uref = AnchorReference(self.name, self.name, self)

    def to_dict(self) -> Dict[str, any]:
        return attr.asdict(self)

    def render_uref(self, label: Optional[str] = None) -> str:
        return self._uref.render_reference(label)

    def render_pointer(self, label: Optional[str] = None) -> str:
        return self._uref.render_pointer(label)

@define
class Metadata:
    device: attr.ib(type=str)
    whoAmI: attr.ib(type=int)
    firmwareVersion: attr.ib(default=None, type=Optional[str])
    hardwareVersion: attr.ib(default=None, type=Optional[str])
    architecture: attr.ib(default=None, type=Optional[str])

    def to_dict(self) -> Dict[str, any]:
        return attr.asdict(self)

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


_COLLECTION_TYPE = List[Register] | List[BitMask] | List[Metadata] | List[IOElement]
_ELEMENT_TYPE = Register | BitMask | Metadata | IOElement


# Collection of multiple elements
class Collection:
    "Parent class that represents a collection of HarpElements"
    def __init__(
        self,
        element_array: Optional[_COLLECTION_TYPE],
        ) -> None:

        self.elements = []
        if element_array:
            self.from_array(element_array)

    def __iter__(self):
        return iter(self.elements)

    def from_array(self, arr: Optional[List[_COLLECTION_TYPE]]) -> None:
        if len(arr) < 1:
            raise ValueError("List can't be empty!")
        for element in arr:
            self.append(element)

    def append(self, element: _ELEMENT_TYPE) -> None:
        self.elements.append(element)

    def insert(self, idx: int, element: _ELEMENT_TYPE) -> None:
        self.elements.insert(idx, element)

    def pop(self, idx: Optional[int]) -> None:
        self.elements.pop(idx)

    def __getitem__(self, index):
        return self.elements[index]