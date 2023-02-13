from __future__ import annotations
import attr

from attr import define
from typing import Optional, List, Dict, Tuple
from enum import Enum

from reflexgenerator.generator.markdown import AnchorReference


# ---------------------------------------------------------------------------- #
#                            Special types and Enums                           #
# ---------------------------------------------------------------------------- #


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
    "NONE", "Command", "Event", "Both"
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


MaskCategory = Enum("MaskType", [
    "BitMask", "GroupMask"
])


def _maskCategory_converter(value: MaskCategory | str) -> MaskCategory:
    if isinstance(value, str):
        return MaskCategory[value]
    if isinstance(value, MaskCategory):
        return value
    raise TypeError("Must be MaskType or str.")


# ---------------------------------------------------------------------------- #
#                                Device metadata                               #
# ---------------------------------------------------------------------------- #


@define
class Metadata:
    device: attr.ib(type=str)
    whoAmI: attr.ib(type=int)
    firmwareVersion: attr.ib(default=None, type=Optional[str])
    hardwareTargets: attr.ib(default=None, type=Optional[str])
    architecture: attr.ib(default=None, type=Optional[str])

    def to_dict(self) -> Dict[str, any]:
        return attr.asdict(self, recurse=False)


# ---------------------------------------------------------------------------- #
#                                     Masks                                    #
# ---------------------------------------------------------------------------- #


_MASKS = {}


@define
class BitOrValue:
    name: attr.ib(default=None, type=Optional[str], converter=str)
    value: attr.ib(default=None, type=Optional[int], converter=hex)

    @classmethod
    def from_dict(self, value_dict: Dict[str, int]):
        assert len(value_dict) == 2
        return BitOrValue(value_dict[0], value_dict[1])


def _make_bitorvalue_array(
        value: Optional[Dict[str, int]]
        ) -> Optional[List[BitOrValue]]:
    if value is None:
        return None
    if isinstance(value, dict):
        return [BitOrValue.from_dict(bit) for bit in value.items()]


@define
class Mask:
    name = attr.ib(type=str, converter=str)
    description = attr.ib(default=None,
                          type=Optional[str], converter=str)
    value = attr.ib(default=None,
                    type=Optional[List[BitOrValue]],
                    converter=_make_bitorvalue_array)
    bits = attr.ib(default=None,
                   type=Optional[List[BitOrValue]],
                   converter=_make_bitorvalue_array)
    maskCategory = attr.ib(default=None,
                           type=Optional[MaskCategory],
                           converter=_maskCategory_converter)
    uid = attr.ib(init=False)

    def __attrs_post_init__(self):
        _MASKS.update({self.name: self})
        self.uid = AnchorReference(self.name, self.name, self)

    def to_dict(self) -> Dict[str, any]:
        return attr.asdict(self, recurse=False)

    @classmethod
    def from_json(self,
                  json_object: Tuple[str, Dict[str, any]],
                  infer_maskCategory=True,
                  maskCategory: Optional[MaskCategory] = None) -> Mask:

        _name = json_object[0]
        if infer_maskCategory:
            if 'bits' in json_object[1]:
                _mask_cat = MaskCategory.BitMask
            elif 'values' in json_object[1]:
                _mask_cat = MaskCategory.GroupMask
            else:
                raise KeyError("Could not infer MaskCategory.\
                                Try to manually assign it.")
            return Mask(name=_name,
                        maskCategory=_mask_cat,
                        **json_object[1])
        else:
            if maskCategory:
                return Mask(name=_name,
                            maskCategory=maskCategory,
                            **json_object[1])
            else:
                raise ValueError("maskCategory cannot be 'None' \
                                 if 'infer_maskCategory' is False")

    def render_uref(self, label: Optional[str] = None) -> str:
        return self.uid.render_reference(label)

    def render_pointer(self, label: Optional[str] = None) -> str:
        return self.uid.render_pointer(label)


def get_mask(value: Optional[str | list[str]]) -> Optional[list[Mask]]:
    if value is None:
        return None
    if isinstance(value, list):
        return [_get_mask_helper(_mask) for _mask in value]
    if isinstance(value, str):
        return [_get_mask_helper(value)]
    raise ValueError("Invalid input format.")


def _get_mask_helper(value: str) -> Mask:
    if value in list(_MASKS.keys()):
        return (_MASKS[value])
    else:
        raise KeyError("Specified mask has not been defined.")


# ---------------------------------------------------------------------------- #
#                                Registers                                     #
# ---------------------------------------------------------------------------- #


@define
class PayloadMember:
    name = attr.ib(type=str)
    mask = attr.ib(type=int, converter=int)
    offset = attr.ib(default=1, type=Optional[int], converter=int)
    maskType = attr.ib(default=None,
                       type=Optional[List[Mask]], converter=get_mask)
    description = attr.ib(default=None, type=Optional[str], converter=str)
    converter = attr.ib(default=None, type=Optional[bool])

    def to_dict(self) -> Dict[str, any]:
        return attr.asdict(self, recurse=False, )

    @classmethod
    def from_json(self,
                  json_object: Tuple[str, Dict[str, any]]) -> PayloadMember:
        _name = json_object[0]
        return PayloadMember(name=_name, **json_object[1])


def _payloadSpec_parser(
        value: Optional[PayloadMember | Tuple[str, Dict[str, any]]]
        ) -> Optional[PayloadMember]:
    if value is None:
        return None
    if isinstance(value, PayloadMember):
        return value
    if isinstance(value, Tuple[str, Dict[str, any]]):
        return PayloadMember.from_json(value)
    raise TypeError("Must be of  \
                    PayloadMember or Tuple[str, Dict[str, any]] type")


@define
class Register:
    name = attr.ib(type=str)
    address = attr.ib(type=int, converter=int)
    payloadType = attr.ib(type=PayloadType | str,
                          converter=_payloadType_converter)
    payloadLength = attr.ib(default=1, type=(str | List[str] | int))
    registerType = attr.ib(default=RegisterType.NONE,
                           type=str, converter=_registerType_converter)
    payloadSpec = attr.ib(default=None,
                          type=Optional[Tuple[str, Dict[str, any]]],
                          converter=_payloadSpec_parser)
    maskType = attr.ib(default=None,
                       type=Optional[List[Mask]], converter=get_mask)
    description = attr.ib(default=None, type=Optional[str], converter=str)
    converter = attr.ib(default=None, type=Optional[bool])
    visibility = attr.ib(default=VisibilityType.Public,
                         type=str, converter=_visibilityType_converter)
    group = attr.ib(default=None, type=Optional[str], converter=str)
    uid = attr.ib(init=False)

    def __attrs_post_init__(self):
        self.uid = AnchorReference(self.name, self.name, self)

    def to_dict(self) -> Dict[str, any]:
        return attr.asdict(self, recurse=False, )

    @classmethod
    def from_json(self,
                  json_object: Tuple[str, Dict[str, any]]) -> Register:

        _name = json_object[0]
        return Register(name=_name, **json_object[1])

    def render_uref(self, label: Optional[str] = None) -> str:
        return self.uid.render_reference(label)

    def render_pointer(self, label: Optional[str] = None) -> str:
        return self.uid.render_pointer(label)


# ---------------------------------------------------------------------------- #
#                                      PinMapping                              #
# ---------------------------------------------------------------------------- #


@define
class PinMap:
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
        return attr.asdict(self, recurse=False)

    @classmethod
    def from_json(self,
                  json_object: Tuple[str, Dict[str, any]]) -> PinMap:

        _name = json_object[0]
        return PinMap(name=_name, **json_object[1])


# ---------------------------------------------------------------------------- #
#                               Collection types                               #
# ---------------------------------------------------------------------------- #


_COLLECTION_TYPE = List[Register] | List[Mask] | List[Metadata] | List[PinMap]
_ELEMENT_TYPE = Register | Mask | Metadata | PinMap


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