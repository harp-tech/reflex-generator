from __future__ import annotations
from typing import Optional, List, Dict
from enum import Enum

# Type shorthands

PayloadType = Enum("PayloadType", [
    "U8", "U16", "U32", "U64",
    "S8", "S16", "S32", "S64",
    "Float"])

ElementType = Enum("ElementType", [
    "NONE", "Mask", "Register"])

RegisterType = Enum("RegisterType", [
    "NONE", "Command", "Event"
])

VisibilityType = Enum("VisibilityType", [
    "Public", "Private"
])

# General parent classes

# Single element
class HarpElement:
    "Parent class that represents a single element (e.g. register or mask)"
    def __init__(
        self,
        name: str,
        payloadType: str | PayloadType,
        alias: Optional[str] = None,
        elementType: ElementType = ElementType.NONE,
        maskType: Optional[str] = None,
        description: Optional[str] = None,
        converter: Optional[bool] = None,
        visibility: str | VisibilityType = VisibilityType.Public
    ) -> None:

        self._elementType = self.elementType = elementType

        self._name = self.name = name
        self._alias = self.alias = alias
        self._payloadType = self.payloadType = payloadType
        self._maskType = self.maskType = maskType
        self._description = self.description = description
        self._converter = self.converter = converter
        self._visibility = self.visibility = visibility

    # Properties

    @property
    def name(self):
        return self._name

    @name.setter
    def name(self, value: str):
        self._name = value

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

    @property
    def elementType(self):
        return self._elementType

    @elementType.setter
    def elementType(self, value: ElementType):
        self._elementType = value

    @property
    def payloadType(self):
        return self._payloadType

    @payloadType.setter
    def payloadType(self, value: str | PayloadType):
        if isinstance(value, PayloadType):
            self._payloadType = value
        elif isinstance(value, str):
            self._payloadType = PayloadType[value]
        else:
            raise TypeError("Only string or PayloadType types are allowed!")

    @property
    def maskType(self):
        return self._maskType

    @maskType.setter
    def maskType(self, value: Optional[str]):
        self._maskType = value

    @property
    def description(self):
        return self._description

    @description.setter
    def description(self, value: Optional[str]):
        self._description = value

    @property
    def converter(self):
        return self._converter

    @converter.setter
    def converter(self, value: Optional[bool]):
        self._converter = value

    @property
    def visibility(self):
        return self._visibility

    @visibility.setter
    def visibility(self, value: str | VisibilityType):
        if isinstance(value, VisibilityType):
            self._visibility = value
        elif isinstance(value, str):
            self._visibility = VisibilityType[value]
        else:
            raise TypeError("Only string or VisibilityType types are allowed!")


# Collection of multiple elements


class ElementCollection:
    "Parent class that represents a collection of HarpElements"
    def __init__(
        self,
        element_array: Optional[List[HarpElement]],
        elementType: ElementType = ElementType.NONE
        ) -> None:

        self.elementType = elementType
        self.elements = []
        if element_array:
            self.from_array(element_array)

    def __iter__(self):
        return iter(self.elements)

    def from_array(self, arr: List[HarpElement]) -> None:
        if len(arr) < 1:
            raise ValueError("List can't be empty!")

        if (self.elementType == ElementType.NONE):
            self.elementType = arr[0].elementType

        if not (self.elementType == arr[0].elementType):
            raise TypeError(
                f"Input list is not of the same element type as collection!\
                    ({arr[0].elementType} and {self.elementType})")

        for element in arr:
            self.append(element)

    def append(self, element: HarpElement) -> None:
        if not (self.elementType == element.elementType):
            raise TypeError(
                f"Element to be appended must\
                     be of the same type as the collection!\
                    ({element.elementType} and {self.elementType})")
        self.elements.append(element)

    def insert(self, idx: int, element: HarpElement) -> None:
        if not (self.elementType == element.elementType):
            raise TypeError(
                f"Element to be appended must\
                     be of the same type as the collection!\
                    ({element.elementType} and {self.elementType})")
        self.elements.insert(idx, element)

    def pop(self, idx: Optional[int]) -> None:
        self.elements.pop(idx)

    def __getitem__(self, index):
        return self.elements[index]


# Child classes

class Mask(HarpElement):

    def __init__(
        self,
        name: str,
        value: str | int,
        payloadType: PayloadType,
        alias: Optional[str] = None,
        maskType: Optional[str] = None,
        description: Optional[str] = None,
        converter: Optional[bool] = None,
        visibility: str | VisibilityType = VisibilityType.Public,
    ) -> None:

        super().__init__(
            name=name,
            alias=alias,
            elementType=ElementType.Mask,
            payloadType=payloadType,
            maskType=maskType,
            description=description,
            converter=converter,
            visibility=visibility
            )

        self._value = self.value = value
        self.dict = None

    @property
    def value(self):
        return self._value

    @value.setter
    def value(self, value: str | int):
        if isinstance(value, int):
            self._value = hex(value)
            return
        if isinstance(value, str):
            if "<<" in value:
                value = value.strip("()")
                value = f"({value})"

            self._value = value
            return

    def __str__(self) -> str:
        att = {k: getattr(self, k) for k, v in
               self.__class__.__dict__.items()
               if isinstance(v, property)}
        att.update({k: getattr(self, k) for k, v in
                    self.__class__.__bases__[0].__dict__.items()
                    if isinstance(v, property)})
        return ("""{}""".format("\n".join([f"{k} : {att[k]}" for k in att]))) + """\n"""

    def to_dict(self) -> Dict[str, any]:
        att = {k: getattr(self, k) for k, v in
               self.__class__.__dict__.items()
               if isinstance(v, property)}
        att.update({k: getattr(self, k) for k, v in
                    self.__class__.__bases__[0].__dict__.items()
                    if isinstance(v, property)})
        return att
class Register(HarpElement):

    def __init__(
        self,
        name: str,
        address: int,
        payloadType: PayloadType,
        alias: Optional[str] = None,
        arrayType: str | List[str] | int = 1,
        registerType: str | RegisterType = RegisterType.NONE,
        maskType: Optional[str] = None,
        description: Optional[str] = None,
        converter: Optional[bool] = None,
        visibility: str | VisibilityType = VisibilityType.Public,
        group: Optional[str] = None
    ) -> None:

        super().__init__(
            name=name,
            alias=alias,
            elementType=ElementType.Register,
            payloadType=payloadType,
            maskType=maskType,
            description=description,
            converter=converter,
            visibility=visibility)

        self._address = self.address = address
        self._registerType = self.registerType = registerType
        self._arrayType = self.arrayType = arrayType
        self._group = self.group = group

    @property
    def address(self):
        return self._address

    @address.setter
    def address(self, value: int):
        self._address = value


    @property
    def arrayType(self):
        return self._arrayType

    @arrayType.setter
    def arrayType(self, value: str | List[str] | int):
        self._arrayType = value


    @property
    def registerType(self):
        return self._registerType

    @registerType.setter
    def registerType(self, value: str | RegisterType):
        if isinstance(value, RegisterType):
            self._registerType = value
        elif isinstance(value, str):
            self._registerType = RegisterType[value]
        else:
            raise TypeError("Only string or RegisterType types are allowed!")

    @property
    def group(self):
        return self._group

    @group.setter
    def group(self, value: Optional[str]):
        self._group = value

    def __str__(self) -> str:
        att = {k: getattr(self, k) for k, v in
               self.__class__.__dict__.items()
               if isinstance(v, property)}
        att.update({k: getattr(self, k) for k, v in
                    self.__class__.__bases__[0].__dict__.items()
                    if isinstance(v, property)})
        return ("""{}""".format("\n".join([f"{k} : {att[k]}" for k in att]))) + """\n"""

    def to_dict(self) -> Dict[str, any]:
        att = {k: getattr(self, k) for k, v in
               self.__class__.__dict__.items()
               if isinstance(v, property)}
        att.update({k: getattr(self, k) for k, v in
                    self.__class__.__bases__[0].__dict__.items()
                    if isinstance(v, property)})
        return att

class MaskCollection(ElementCollection):

    def __init__(
            self,
            element_array: Optional[List[Mask]]) -> None:
        super().__init__(
            elementType=ElementType.Mask,
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
            elementType=ElementType.Register,
            element_array=element_array)

    def from_array(self, arr: List[Register]) -> None:
        super().from_array(arr)

    def append(self, element: Register) -> None:
        super().append(element)

    def insert(self, idx: int, element: Register) -> None:
        super().insert(idx, element)

    def pop(self, idx: Optional[int]) -> None:
        super().pop(idx)
