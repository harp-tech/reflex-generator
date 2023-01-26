from __future__ import annotations
from typing import Optional, List, Dict
from enum import Enum

# Type shorthands

HarpDataType = Enum("HarpDataType", [
    "NONE",
    "U8", "U16", "U32", "U64",
    "S8", "S16", "S32", "S64",
    "float"])

ElementType = Enum("ElementType", [
    "NONE", "Mask", "Register"])


# General parent classes

# Single element
class HarpElement:
    "Parent class that represents a single element (e.g. register or mask)"
    def __init__(
        self,
        name: str,
        alias: Optional[str] = None,
        element_type: ElementType = ElementType.NONE,
        dtype: HarpDataType = HarpDataType.NONE,
        mask_family: Optional[str] = None,
        description: Optional[str] = None,
        converter: Optional[str] = None,
        enable_generator: bool = True,
        grouping: Optional[str] = None
    ) -> None:

        self._name = name
        self._alias = alias
        self._element_type = element_type
        self._dtype = dtype
        self._mask_family = mask_family
        self._description = description
        self._converter = converter
        self._enable_generator = enable_generator
        self._grouping = grouping
        self.dict = {}

    def _setter_callback(self):
        pass

    # Properties

    @property
    def name(self):
        return self._name

    @name.setter
    def name(self, value: str):
        self._name = value
        self._setter_callback()

    @property
    def alias(self):
        return self._alias

    @alias.getter
    def alias(self):
        if self._alias is None:
            return self._name
        else:
            return self._alias

    @alias.setter
    def alias(self, value: str):
        self._alias = value
        self._setter_callback()

    @property
    def element_type(self):
        return self._element_type

    @element_type.setter
    def element_type(self, value: ElementType):
        self._element_type = value
        self._setter_callback()

    @property
    def dtype(self):
        return self._dtype

    @dtype.setter
    def dtype(self, value: HarpDataType):
        self._dtype = value
        self._setter_callback()

    @property
    def mask_family(self):
        return self._mask_family

    @mask_family.setter
    def mask_family(self, value: Optional[str]):
        self._mask_family = value
        self._setter_callback()

    @property
    def description(self):
        return self._description

    @description.setter
    def description(self, value: Optional[str]):
        self._description = value
        self._setter_callback()

    @property
    def converter(self):
        return self._converter

    @converter.setter
    def converter(self, value: Optional[str]):
        self._converter = value
        self._setter_callback()

    @property
    def enable_generator(self):
        return self._enable_generator

    @enable_generator.setter
    def enable_generator(self, value: Optional[str]):
        self._enable_generator = value
        self._setter_callback()

    @property
    def grouping(self):
        return self._grouping

    @grouping.setter
    def grouping(self, value: Optional[str]):
        self._grouping = value
        self._setter_callback()

    # Methods

    def __str__(self) -> str:
        _l = [f"{k} : {v}" for k,v in self.dict.items()]
        return ("""{}""".format("\n".join(_l))) + """\n"""


# Collection of multiple elements


class ElementCollection:
    "Parent class that represents a collection of HarpElements"
    def __init__(
        self,
        element_array: Optional[List[HarpElement]],
        element_type: ElementType = ElementType.NONE
        ) -> None:

        self.element_type = element_type
        self.elements = []
        if element_array:
            self.from_array(element_array)

    def __iter__(self):
        return iter(self.elements)

    def from_array(self, arr: List[HarpElement]) -> None:
        if len(arr) < 1:
            raise ValueError("List can't be empty!")

        if (self.element_type == ElementType.NONE):
            self.element_type = arr[0].element_type

        if not (self.element_type == arr[0].element_type):
            raise TypeError(
                f"Input list is not of the same element type as collection!\
                    ({arr[0].element_type} and {self.element_type})")

        for element in arr:
            self.append(element)

    def append(self, element: HarpElement) -> None:
        if not (self.element_type == element.element_type):
            raise TypeError(
                f"Element to be appended must be of the same type as the collection!\
                    ({element.element_type} and {self.element_type})")
        self.elements.append(element)

    def insert(self, idx: int, element: HarpElement) -> None:
        if not (self.element_type == element.element_type):
            raise TypeError(
                f"Element to be appended must be of the same type as the collection!\
                    ({element.element_type} and {self.element_type})")
        self.elements.insert(idx, element)

    def pop(self, idx: Optional[int]) -> None:
        self.elements.pop(idx)


# Child classes

class Mask(HarpElement):

    def __init__(
        self,
        name: str,
        mask: str,
        dtype: HarpDataType,
        alias: Optional[str] = None,
        mask_family: Optional[str] = None,
        description: Optional[str] = None,
        converter: Optional[str] = None,
        enable_generator: bool = True,
        grouping: Optional[str] = None
    ) -> None:

        super().__init__(
            name=name,
            alias=alias,
            element_type=ElementType.Mask,
            dtype=dtype,
            mask_family=mask_family,
            description=description,
            converter=converter,
            enable_generator=enable_generator,
            grouping=grouping)

        self._mask = self.mask = mask
        self.dict = None
        self._refresh_property_dict()

    # override parent method for setter
    def _setter_callback(self):
        self._refresh_property_dict()

    @property
    def mask(self):
        return self._mask

    @mask.setter
    def mask(self, value: str):
        if "<<" in value:
            value = value.strip("()")
            value = f"({value})"

        self._mask = value
        self._setter_callback()

    def _refresh_property_dict(self):
        self.dict = {
            "element_type": self.element_type,
            "name": self.name,
            "alias": self.alias,
            "mask": self.mask,
            "mask_family": self.mask_family,
            "dtype": self.dtype,
            "description": self.description,
            "converter": self.converter,
            "enable_generator": self.enable_generator,
            "grouping": self.grouping,
        }

    @staticmethod
    def from_dict(dict_: Dict[str, any]) -> Mask:
        return Mask(**dict_)


class Register(HarpElement):

    def __init__(
        self,
        name: str,
        address: int,
        dtype: HarpDataType,
        alias: Optional[str] = None,
        array_spec: Optional[str] = None,
        is_event: bool = False,
        mask_family: Optional[str] = None,
        description: Optional[str] = None,
        converter: Optional[str] = None,
        enable_generator: bool = True,
        grouping: Optional[str] = None
    ) -> None:

        super().__init__(
            name=name,
            alias=alias,
            element_type=ElementType.Register,
            dtype=dtype,
            mask_family=mask_family,
            description=description,
            converter=converter,
            enable_generator=enable_generator,
            grouping=grouping)

        self._address = address
        self._is_event = is_event
        self._array_spec = array_spec
        self._refresh_property_dict()

    # override parent method for setter
    def _setter_callback(self):
        self._refresh_property_dict()

    @property
    def address(self):
        return self._address

    @address.setter
    def address(self, value: int):
        self._address = value
        self._setter_callback()

    @property
    def array_spec(self):
        return self._array_spec

    @array_spec.setter
    def array_spec(self, value: Optional[str]):
        self._array_spec = value
        self._setter_callback()

    @property
    def is_event(self):
        return self._is_event

    @is_event.setter
    def is_event(self, value: bool):
        self._is_event = value
        self._setter_callback()

    def _refresh_property_dict(self):
        self.dict = {
            "element_type": self.element_type,
            "name": self.name,
            "alias": self.alias,
            "address": self.address,
            "dtype": self.dtype,
            "array_spec": self.array_spec,
            "is_event": self.is_event,
            "description": self.description,
            "mask_family": self.mask_family,
            "converter": self.converter,
            "enable_generator": self.enable_generator,
            "grouping": self.grouping
        }

    @staticmethod
    def from_dict(dict_: Dict[str, any]) -> Register:
        return Register(**dict_)


class MaskCollection(ElementCollection):

    def __init__(
            self,
            element_array: Optional[List[Mask]]) -> None:
        super().__init__(
            element_type=ElementType.Mask,
            element_array=element_array)

    def from_array(self, arr: List[Mask]) -> None:
        super().from_array(arr)

    def append(self, element: Mask) -> None:
        super().append(element)

    def insert(self, idx: int, element: Mask) -> None:
        super().insert(idx, element)

    def pop(self, idx: Optional[int]) -> None:
        super().pop(idx)


class RegisterCollection(ElementCollection):

    def __init__(
            self,
            element_array: Optional[List[Register]]) -> None:
        super().__init__(
            element_type=ElementType.Register,
            element_array=element_array)

    def from_array(self, arr: List[Register]) -> None:
        super().from_array(arr)

    def append(self, element: Register) -> None:
        super().append(element)

    def insert(self, idx: int, element: Register) -> None:
        super().insert(idx, element)

    def pop(self, idx: Optional[int]) -> None:
        super().pop(idx)
