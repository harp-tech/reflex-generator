from __future__ import annotations
import attr

from attr import define
from numbers import Number
from enum import Enum
from typing import (
    Optional,
    List,
    Dict,
    Tuple,
    List)
from reflexgenerator.generator.xref import (
    UidReference,
    make_anchor,
    create_link)


# ---------------------------------------------------------------------------- #
#                            Special types and Enums                           #
# ---------------------------------------------------------------------------- #


class BaseEnum(Enum):
    """Implements the abstract enumerator class to be used in all enumerators.
    """

    def __str__(self) -> str:
        return self.generate_link_reference()

    def __repr__(self) -> str:
        return self.generate_link_reference()

    @classmethod
    def _format_ref(self, value) -> str:
        return f"ref-{self.__name__}-{value}"

    @classmethod
    def generate_anchor_references(self) -> List[str]:
        return [make_anchor(
            self._format_ref(entry.name), entry.name
            ) for entry in self]

    @classmethod
    def format_anchor_references(self):

        def _formater(value: str) -> str:
            return f"""- {value}\n\n"""

        return f"""### {make_anchor(
            self._format_ref(self.__name__),
            self.__name__)}\n""" + "".join(
                [_formater(it) for it in self.generate_anchor_references()])

    def generate_link_reference(self) -> str:
        return create_link(self._format_ref(self.name), self.name)


class PayloadType(BaseEnum):
    U8 = "U8"
    U16 = "U16"
    U32 = "U32"
    U64 = "U64"
    S8 = "S8"
    S16 = "S16"
    S32 = "S32"
    S64 = "S64"
    Float = "Float"


def _payloadType_converter(value: PayloadType | str) -> PayloadType:
    if isinstance(value, str):
        return PayloadType[value]
    if isinstance(value, PayloadType):
        return value
    raise TypeError("Must be PayloadType or str.")


class AccessType(BaseEnum):
    Read = "Read"
    Write = "Write"
    Event = "Event"


def _accessType_converter(
        value: AccessType | str | List[AccessType, str]
        ) -> List[AccessType]:

    def _singleton_converter(value: AccessType | str) -> AccessType:
        if isinstance(value, str):
            return AccessType[value]
        if isinstance(value, AccessType):
            return value
        raise TypeError("Must be AccessType or str.")

    if isinstance(value, list):
        return [_singleton_converter(it) for it in value]
    elif isinstance(value, str) or isinstance(value, AccessType):
        return [_singleton_converter(value)]
    else:
        raise TypeError(
            "Must be AccessType, str, or a List with these elements.")


class VisibilityType(BaseEnum):
    Public = "Public"
    Private = "Private"


def _visibilityType_converter(
        value: VisibilityType | str
        ) -> VisibilityType:
    if isinstance(value, str):
        return VisibilityType[value]
    if isinstance(value, VisibilityType):
        return value
    raise TypeError("Must be VisibilityType or str.")


class VolatilityType(BaseEnum):
    Yes = "Yes"
    No = "No"


def _volatilityType_converter(
        value: VolatilityType | str | bool
        ) -> VolatilityType:
    if isinstance(value, str):
        return VolatilityType[value]
    if isinstance(value, VolatilityType):
        return value
    if isinstance(value, bool):
        if value:
            return VolatilityType.Yes
        else:
            return VolatilityType.No
    raise TypeError("Must be VolatilityType or str.")


class MaskCategory(BaseEnum):
    BitMask = "BitMask"
    GroupMask = "GroupMask"


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
    device = attr.ib(type=str)
    whoAmI = attr.ib(type=int)
    firmwareVersion = attr.ib(default=None, type=Optional[str])
    hardwareTargets = attr.ib(default=None, type=Optional[str])
    architecture = attr.ib(default=None, type=Optional[str])
    uid = attr.ib(default=None, type=Optional[UidReference])

    def __attrs_post_init__(self):
        if self.uid is None:
            self.uid = UidReference(self)

    @property
    def name(self) -> str:
        return f"{self.device}_{self.whoAmI}"

    def to_dict(self) -> Dict[str, any]:
        return attr.asdict(self, recurse=False)

    def format_dict(self) -> str:
        return ("".join(
            [f'''<font size="4"> - {k}: {v} </font> \n\n'''
             for k, v in self.to_dict().items()
             if k != "uid"]))

# ---------------------------------------------------------------------------- #
#                                     Masks                                    #
# ---------------------------------------------------------------------------- #


_MASKS = {}


@define
class BitOrValue:
    name = attr.ib(default=None, type=Optional[str], converter=str)
    value = attr.ib(default=None, type=Optional[int], converter=hex)
    description = attr.ib(default=None, type=Optional[str], converter=str)
    uid = attr.ib(default=None, type=Optional[UidReference])

    def __attrs_post_init__(self):
        if self.uid is None:
            self.uid = UidReference(self)

    @classmethod
    def parse(self,
              value: Tuple[str, Dict[Number | str, Optional[str]]]
              ) -> BitOrValue:
        _name = value[0]

        if isinstance(value[1], Dict):  # allow optional parameters
            _value = list(value[1].keys())[0]
            try:
                _description = value[1]["description"]
            except KeyError:
                _value = value[1]
                _description = None
        else:  # Assume only the value has been passed
            _value = value[1]
            _description = None
        return BitOrValue(
            name=_name,
            value=_value,
            description=_description)

    def to_dict(self) -> Dict[str, any]:
        return attr.asdict(self, recurse=False)

    def __str__(self) -> str:
        # return self.uid.render_pointer(self.name)
        return self.format_dict()

    def __repr__(self) -> str:
        return self.format_dict()

    def format_dict(self) -> str:
        return (f"""*{self.name}*\n
        \tvalue = {self.value}\n
        \tdescription = {self.description}""")


def _make_bitorvalue_array(
        value: Optional[Dict[str, int]]
        ) -> Optional[List[BitOrValue]]:
    if value is None:
        return None
    if isinstance(value, dict):
        return [BitOrValue.parse(bit) for bit in value.items()]


@define
class Mask:
    name = attr.ib(type=str, converter=str)
    description = attr.ib(default=None,
                          type=Optional[str], converter=str)
    values = attr.ib(default=None,
                     type=Optional[List[BitOrValue]],
                     converter=_make_bitorvalue_array)
    bits = attr.ib(default=None,
                   type=Optional[List[BitOrValue]],
                   converter=_make_bitorvalue_array)
    maskCategory = attr.ib(default=None,
                           type=Optional[MaskCategory],
                           converter=_maskCategory_converter)
    uid = attr.ib(default=None, type=Optional[UidReference])

    def __attrs_post_init__(self):
        _MASKS.update({self.name: self})
        if self.uid is None:
            self.uid = UidReference(self)

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

    def format_dict(self) -> str:
        if self.maskCategory == MaskCategory.BitMask:
            _attr = self.bits
            _f = "bits"
        elif self.maskCategory == MaskCategory.GroupMask:
            _attr = self.values
            _f = "values"
        else:
            raise ValueError("Invalid MaskCategory.")

        _param_text = f"""> description = {self.description} \n\n"""
        if _attr is not None:
            _param_text += f"""> {_f} = \n\n"""
            for value in _attr:
                _param_text += "\n * " + value.format_dict() + "\n"
        return f"""### {self.uid.render_reference(self.name)}\n{_param_text}"""

    def __str__(self) -> str:
        return self.uid.render_pointer(self.name)

    def __repr__(self) -> str:
        return self.uid.render_pointer(self.name)


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
        raise KeyError(f"Specified mask, {value}, has not been defined.")


# ---------------------------------------------------------------------------- #
#                                Registers                                     #
# ---------------------------------------------------------------------------- #


@define
class PayloadMember:
    name = attr.ib(type=str)
    mask = attr.ib(default=None, type=Optional[int],
                   converter=lambda value: int(value)
                   if value is not None else None)
    offset = attr.ib(default=1, type=Optional[int], converter=int)
    maskType = attr.ib(default=None,
                       type=Optional[List[Mask]], converter=get_mask)
    description = attr.ib(default=None, type=Optional[str], converter=str)
    converter = attr.ib(default=None, type=Optional[bool])
    defaultValue = attr.ib(default=None, type=Optional[Number])
    maxValue = attr.ib(default=None, type=Optional[Number])
    minValue = attr.ib(default=None, type=Optional[Number])
    interfaceType = attr.ib(default=None, type=Optional[str])
    uid = attr.ib(default=None, type=Optional[UidReference])

    def __attrs_post_init__(self):
        if self.uid is None:
            self.uid = UidReference(self)

    def to_dict(self) -> Dict[str, any]:
        return attr.asdict(self, recurse=False, )

    @classmethod
    def from_json(self,
                  json_object: Tuple[str, Dict[str, any]]) -> PayloadMember:
        _name = json_object[0]
        return PayloadMember(name=_name, **json_object[1])

    def format_dict(self) -> str:
        _param_text = ("".join([f"""> {k} = {v} \n\n""" for
                                k, v in self.to_dict().items()
                                if k not in ["uid", "name"]]))
        return f"""### {self.uid.render_reference(self.name)}\n{_param_text}"""

    def __str__(self) -> str:
        return self.uid.render_pointer(self.name)

    def __repr__(self) -> str:
        return self.uid.render_pointer(self.name)


def _payloadSpec_parser(
        value: Optional[List[PayloadMember] | PayloadMember | Dict[str, any]]
        ) -> Optional[List[PayloadMember]]:
    if value is None:
        return None
    if isinstance(value, PayloadMember):
        return [value]
    if isinstance(value, list):
        return value
    if isinstance(value, dict):
        return [PayloadMember.from_json(s) for s in value.items()]
    print(value, type(value))
    raise TypeError("Unexpected input type.")


@define
class Register:
    name = attr.ib(type=str)
    address = attr.ib(type=int, converter=int)
    type = attr.ib(type=PayloadType | str,
                   converter=_payloadType_converter)
    length = attr.ib(default=1, type=int, converter=int)
    access = attr.ib(default=[AccessType.Read],
                     type=str | List[str],
                     converter=_accessType_converter)
    payloadSpec = attr.ib(default=None,
                          type=Optional[Dict[str, any]],
                          converter=_payloadSpec_parser)
    maskType = attr.ib(default=None,
                       type=Optional[List[Mask]], converter=get_mask)
    description = attr.ib(default=None, type=Optional[str], converter=str)
    converter = attr.ib(default=None, type=Optional[bool])
    defaultValue = attr.ib(default=None, type=Optional[Number])
    maxValue = attr.ib(default=None, type=Optional[Number])
    minValue = attr.ib(default=None, type=Optional[Number])
    interfaceType = attr.ib(default=None, type=Optional[str])
    visibility = attr.ib(default=VisibilityType.Public,
                         type=str, converter=_visibilityType_converter)
    volatile = attr.ib(default=VolatilityType.Yes,
                       type=str | bool, converter=_volatilityType_converter)
    group = attr.ib(default=None, type=Optional[str], converter=str)
    uid = attr.ib(default=None, type=Optional[UidReference])

    def __attrs_post_init__(self):
        if (AccessType.Read not in self.access):
            self.access.insert(0, AccessType.Read)
        if self.uid is None:
            self.uid = UidReference(self)

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

    def format_dict(self) -> str:
        _param_text = ("".join([f"""> {k} = {v} \n\n""" for
                                k, v in self.to_dict().items()
                                if k not in ["uid", "name"]]))
        return f"""### {self.uid.render_reference(self.name)}\n{_param_text}"""

    def __str__(self) -> str:
        return self.uid.render_pointer(self.name)

# ---------------------------------------------------------------------------- #
#                                      PinMapping                              #
# ---------------------------------------------------------------------------- #


class DirectionType(BaseEnum):
    input = "input"
    output = "output"


def _directionType_converter(
        value: DirectionType | str
        ) -> DirectionType:
    if isinstance(value, str):
        return DirectionType[value]
    if isinstance(value, DirectionType):
        return value
    raise TypeError("Must be DirectionType or str.")


class InputPinModeType(BaseEnum):
    pullup = "pullup"
    pulldown = "pulldown"
    tristate = "tristate"
    busholder = "busholder"


def _inputPinModeType_converter(
        value: InputPinModeType | str
        ) -> InputPinModeType:
    if isinstance(value, str):
        return InputPinModeType[value]
    if isinstance(value, InputPinModeType):
        return value
    raise TypeError("Must be InputPinModeType or str.")


class TriggerModeType(BaseEnum):
    none = "none"
    rising = "rising"
    falling = "falling"
    toggle = "toggle"
    low = "low"


def _triggerModeType_converter(
        value: TriggerModeType | str
        ) -> TriggerModeType:
    if isinstance(value, str):
        return TriggerModeType[value]
    if isinstance(value, TriggerModeType):
        return value
    raise TypeError("Must be TriggerModeType or str.")


class InterruptPriorityType(BaseEnum):
    off = "off"
    low = "low"
    medium = "medium"
    high = "high"


def _interruptPriorityType_converter(
        value: InterruptPriorityType | str
        ) -> InterruptPriorityType:
    if isinstance(value, str):
        return InterruptPriorityType[value]
    if isinstance(value, InterruptPriorityType):
        return value
    raise TypeError("Must be InterruptPriorityType or str.")


class OutputPinModeType(BaseEnum):
    wiredOr = "wiredOr"
    wiredAnd = "wiredAnd"
    wiredOrPull = "wiredOrPull"
    wiredAndPull = "wiredAndPull"


def _outputPinModeType_converter(
        value: OutputPinModeType | str
        ) -> OutputPinModeType:
    if isinstance(value, str):
        return OutputPinModeType[value]
    if isinstance(value, OutputPinModeType):
        return value
    raise TypeError("Must be OutputPinModeType or str.")


class InitialStateType(BaseEnum):
    low = "low"
    high = "high"


def _initialStateType_converter(
        value: InitialStateType | str
        ) -> InitialStateType:
    if isinstance(value, str):
        return InitialStateType[value]
    if isinstance(value, InitialStateType):
        return value
    raise TypeError("Must be InitialStateType or str.")


def PinMap(dict_args) -> InputPin | OutputPin:
    if "direction" not in dict_args:
        raise KeyError("Key 'direction' not found.")
    if dict_args["direction"] == "input":
        return InputPin(**dict_args)
    elif dict_args["direction"] == "output":
        return OutputPin(**dict_args)
    else:
        raise ValueError("Invalid value for 'direction'.")


def PinMap_from_json(
        json_object: Tuple[str, Dict[str, any]]
        ) -> InputPin | OutputPin:
    if "direction" not in json_object[1]:
        raise KeyError("Key 'direction' not found.")
    if json_object[1]["direction"] == "input":
        return InputPin.from_json(json_object)
    elif json_object[1]["direction"] == "output":
        return OutputPin.from_json(json_object)
    else:
        raise ValueError("Invalid value for 'direction'.")


@define
class InputPin:
    name = attr.ib(type=str)
    port = attr.ib(type=str)
    pinNumber = attr.ib(type=int, converter=int)
    direction = attr.ib(type=str, converter=_directionType_converter)
    pinMode = attr.ib(type=str, converter=_inputPinModeType_converter)
    triggerMode = attr.ib(type=str, converter=_triggerModeType_converter)
    interruptPriority = attr.ib(
        type=str,
        converter=_interruptPriorityType_converter)
    interruptNumber = attr.ib(type=int, converter=int)
    description = attr.ib(default=None, type=Optional[str], converter=str)
    uid = attr.ib(default=None, type=Optional[UidReference])

    def __attrs_post_init__(self):
        if self.uid is None:
            self.uid = UidReference(self)

    def to_dict(self):
        return attr.asdict(self, recurse=True)

    @classmethod
    def from_json(self,
                  json_object: Tuple[str, Dict[str, any]]) -> InputPin:

        _name = json_object[0]
        return InputPin(name=_name, **json_object[1])

    def format_dict(self) -> str:
        _param_text = ("".join([f"""> {k} = {v} \n\n""" for
                                k, v in self.to_dict().items()
                                if k not in ["uid", "name"]]))
        return f"""### {self.uid.render_reference(self.name)}\n{_param_text}"""

    def __str__(self) -> str:
        return self.uid.render_pointer(self.name)


@define
class OutputPin:
    name = attr.ib(type=str)
    port = attr.ib(type=str)
    pinNumber = attr.ib(type=int, converter=int)
    direction = attr.ib(type=str, converter=_directionType_converter)
    allowRead = attr.ib(type=bool, converter=bool)
    pinMode = attr.ib(type=str, converter=_outputPinModeType_converter)
    initialState = attr.ib(type=int, converter=_initialStateType_converter)
    invert = attr.ib(type=bool, converter=bool)
    description = attr.ib(default=None, type=Optional[str], converter=str)
    uid = attr.ib(default=None, type=Optional[UidReference])

    def __attrs_post_init__(self):
        if self.uid is None:
            self.uid = UidReference(self)

    def to_dict(self):
        return attr.asdict(self, recurse=True)

    @classmethod
    def from_json(self,
                  json_object: Tuple[str, Dict[str, any]]) -> OutputPin:

        _name = json_object[0]
        return OutputPin(name=_name, **json_object[1])

    def format_dict(self) -> str:
        _param_text = ("".join([f"""> {k} = {v} \n\n""" for
                                k, v in self.to_dict().items()
                                if k not in ["uid", "name"]]))
        return f"""### {self.uid.render_reference(self.name)}\n{_param_text}"""

    def __str__(self) -> str:
        return self.uid.render_pointer(self.name)
# ---------------------------------------------------------------------------- #
#                               Collection types                               #
# ---------------------------------------------------------------------------- #


_COLLECTION_TYPE = List[Register] | List[Mask] | List[Metadata] | List[InputPin | OutputPin]
_ELEMENT_TYPE = Register | Mask | Metadata | InputPin | OutputPin


class Collection:
    "Parent class that represents a collection of HarpElements"
    def __init__(
            self,
            element_array: Optional[_COLLECTION_TYPE]) -> None:

        self.elements = []
        if element_array:
            self.from_array(element_array)

    def __iter__(self):
        return iter(self.elements)

    def from_array(self, arr: Optional[_COLLECTION_TYPE]) -> None:
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