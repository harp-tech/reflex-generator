from __future__ import annotations
from typing import Optional, List, Callable
from enum import Enum

# Type shorthands

HarpDataType = Enum("HarpDataType",[
    "NONE",
    "U8", "U16", "U32", "U64",
    "S8", "S16", "S32", "S64",
    "float"
    ])

ElementType = Enum("ElementType",[
    "NONE", "Mask", "Register"
])


# General parent classes

# Single element
class HarpElement:
    "Parent class that represents a single element (e.g. register or mask)"
    def __init__(
        self,
        name: str,
        element_type: ElementType = ElementType.NONE,
        dtype: HarpDataType = HarpDataType.NONE,
        mask_family: Optional[str] = None,
        description: Optional[str] = None,
        converter: Optional[str] = None,
        enable_generator: bool = True,
    ) -> None:

        self._name = name
        self._element_type = element_type
        self._dtype = dtype
        self._mask_family = mask_family
        self._description = description
        self._converter = converter
        self._enable_generator = enable_generator
        self.dict = {}

    def _setter_callback(self):
        pass

    @property
    def name(self):
        return self._name
    @name.setter
    def name(self, value: str):
        self._name = value
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

    def __str__(self) -> str:
        _l = [f"{k} : {v}" for k,v in self.dict.items()]
        return ("""{}""".format("\n".join(_l))) + """\n"""

# Collection of multiple elements
class ElementCollection:
    "Parent class that represents a collection of HarpElements"
    def __init__(
        self,
        element_type: ElementType = ElementType.NONE,
        element_array = Optional[List[HarpElement]]):

        self.element_type = element_type
        self.elements = []
        if element_array:
            self.from_array(element_array)

    def __iter__(self):
        return iter(self.elements)

    def from_array(self, arr: List[HarpElement]) -> None:
        if len(arr) < 1:
            raise ValueError("List can't be empty!")

        if self.element_type == ElementType.NONE:
            self.element_type = arr[0].element_type

        if not(self.element_type==arr[0].element_type):
            raise TypeError(
                f"Input list is not of the same element type as collection!\
                    ({arr[0].element_type} and {self.element_type})")

        for element in arr:
            self.append(element)

    def append(self, element: HarpElement) -> None:
        if not(self.element_type == element.element_type):
            raise TypeError(
                f"Element to be appended must be of the same type as the collection!\
                    ({element.element_type} and {self.element_type})")
        self.elements.append(element)

    def insert(self, idx: int, element: HarpElement) -> None:
        if not(self.element_type == element.element_type):
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
        mask_family: Optional[str] = None,
        description: Optional[str] = None,
        converter: Optional[str] = None,
        enable_generator: bool = True
    ) -> None:

        super().__init__(
            name=name,
            element_type=ElementType.Mask,
            dtype=dtype,
            mask_family=mask_family,
            description=description,
            converter=converter,
            enable_generator=enable_generator)

        self._mask = mask
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
            "Name": self.name,
            "MaskFamily": self.mask_family,
            "Mask": self.mask,
            "Format": self.dtype,
            "Description": self.description,
            "Converter": self.converter,
            "AutoInterface": self.enable_generator
        }

class Register(HarpElement):

    def __init__(
        self,
        name: str,
        address: int,
        dtype: HarpDataType,
        arrayspec: Optional[str] = None,
        is_event: bool = False,
        mask_family: Optional[str] = None,
        description: Optional[str] = None,
        converter: Optional[str] = None,
        enable_generator: bool = True
    ) -> None:

        super().__init__(
            name=name,
            element_type=ElementType.Register,
            dtype=dtype,
            mask_family=mask_family,
            description=description,
            converter=converter,
            enable_generator=enable_generator)

        self._address = address
        self._is_event = is_event
        self._array_spec = arrayspec
        self._refresh_property_dict()

    # override parent method for setter
    def _setter_callback(self):
        self._refresh_property_dict()

    @property
    def address(self):
        return self._address
    @address.setter
    def address(self, value:int):
        self._address = value
        self._setter_callback()

    @property
    def array_spec(self):
        return self._array_spec
    @array_spec.setter
    def array_spec(self, value:Optional[str]):
        self._array_spec = value
        self._setter_callback()

    @property
    def is_event(self):
        return self._is_event
    @is_event.setter
    def is_event(self, value:bool):
        self._is_event = value
        self._setter_callback()


    def _refresh_property_dict(self):
        self.dict = {
            "Name": self.name,
            "Address": self.address,
            "Format": self.dtype,
            "PayloadFormat": self.array_spec,
            "Description": self.description,
            "MaskFamily": self.mask_family,
            "Converter": self.converter,
            "AutoInterface": self.enable_generator
        }


class MaskCollection(ElementCollection):

    def __init__(
            self,
            element_array = Optional[List[Mask]]) -> None:
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
            element_array = Optional[List[Register]]) -> None:
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
