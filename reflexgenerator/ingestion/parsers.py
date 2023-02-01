from __future__ import annotations
from enum import Enum
from typing import Dict, Optional, List, Callable, Tuple
import pandas as pd

from reflexgenerator.sources import HarpMessageType, HarpDataType

DEFAULT_ARRAY_DEL = ";"

class ColumnParser:
    def __init__(
        self,
        api_name: str,
        column_name: str,
        converter: Optional[Callable] = None,
        allow_none: bool = True) -> None:

        self._api_name = api_name
        self._column_name = column_name
        self._converter = converter
        self._allow_none = allow_none

    @property
    def column_name(self):
        return self._column_name

    @property
    def api_name(self):
        return self._api_name

    @property
    def converter(self):
        return self._converter

    @property
    def allow_none(self):
        return self._allow_none


# Auxiliary parsing functions

def _split_to_list(
    full_string: str,
    delimiter: str = DEFAULT_ARRAY_DEL
    ) -> List[str]:

    _str = full_string.split(sep=delimiter)
    _str = [x for x in _str if x != ""]
    if len(_str) < 1:
        raise IndexError(
            f"List is empty after splitting string\
                 using {delimiter} as separator")
    else:
        return _str


def _empty_or_none(
    nullable_string: Optional[str]
    ) -> bool:

    if (nullable_string is None) or (nullable_string == ""):
        return True
    else:
        return False


def parse_converter(
    value: Optional[str],
    delimiter: str = DEFAULT_ARRAY_DEL
    ) -> Optional[List[str]]:

    if not(_empty_or_none(value)):
        _str = _split_to_list(value, delimiter)
        return _str
    else:
        return None


def parse_reg_array_spec(
    value: Optional[str],
    delimiter: str = DEFAULT_ARRAY_DEL
    ) -> Tuple[int, Optional[List[str]]]:
    """Parses a string with the specs of an HarpMessage Array

    Args:
        nullable_string (Optional[str]): string to be parsed.
        delimiter (str, optional): Delimiter to be used to split\
            the string with potentially multiple entries. Defaults to DEFAULT_ARRAY_DEL.

    Returns:
        Tuple[int, Optional[List[str]]]: Tuple with the size of the\
            array (int) and, if found, list of strings. Must be greater than 0.
            If only a int is provided in the field, None will be returned\
                instead of a list of strings.
    """
    if (_empty_or_none(value)):
        return (1, None)
    _try_int = _int_try_parse_or_none(value, 1)
    if _try_int is not None:
        # if the input is None or a single parsable int
        return (_try_int, None)
    if value:
        _arr = _split_to_list(value, delimiter)
        if len(_arr) > 0:
            return (len(_arr), _arr)
        else:
            raise ValueError("Length of array cannot be 0.")


def _int_try_parse_or_none(
    value: Optional[str],
    none_default: int = 1
    ) -> Optional[int]:

    if value is None:
        return none_default
    try:
        return int(value)
    except ValueError:
        return None


def parse_address(value: Optional[str]) -> int:
    value = int(value)
    if ((value > 255) or (value < 0)):
        raise ValueError("Address value must be an integer in the range [0:255]")
    return value



def parse_message_type(
    value: Optional[str],
    delimiter=DEFAULT_ARRAY_DEL
    ) -> Optional[List[HarpMessageType]]:

    if (_empty_or_none(value)):
        return None
    value = _split_to_list(value, delimiter)

    ret = []
    if value is not None:
        for m_type in value:
            match m_type.casefold():
                case "read":
                    ret.append(HarpMessageType.READ)
                case "write":
                    ret.append(HarpMessageType.WRITE)
                case "event":
                    ret.append(HarpMessageType.EVENT)
                case _:
                    raise ValueError(f"Unknown Harp message type {m_type}")
        return ret
    else:
        return None


def _validate_single_string(value: str) -> bool:
    _invalid_characters = " !£$%^&*()-=+{}[]#~@?><.,/¬`"
    return not(any([char in value for char in _invalid_characters]))

def parse_name(value: Optional[str]) -> Optional[str]:
    if _empty_or_none(value):
        return None
    if _validate_single_string(value):
        return value
    else:
        raise ValueError(f"Found invalid characters in name {value}")


def parse_alias(value: Optional[str]) -> Optional[str]:
    if _empty_or_none(value):
        return None
    if _validate_single_string(value):
        return value
    else:
        raise ValueError(f"Found invalid characters in alias {value}")


def parse_mask_family(value: Optional[str]) -> Optional[str]:
    if _empty_or_none(value):
        return None
    if _validate_single_string(value):
        return value
    else:
        raise ValueError(f"Found invalid characters in mask group m {value}")


def parse_dtype(value: Optional[str]) -> Optional[HarpDataType]:
    if _empty_or_none(value):
        return None
    else:
        try:
            return HarpDataType[value]
        except KeyError:
            raise KeyError(f"Uknown harp data type {value}")


def parse_enabled_generator(value: Optional[str]) -> Optional[bool]:
    if _empty_or_none:
        return None
    else:
        if value.casefold() in ["1", "true"]:
            return True
        elif value.casefold() in ["0", "false"]:
            return False
        else:
            raise ValueError(f"Enabled generator must be True(1) or Fals(0).\
                 Found {value}")
def parse_grouping(value: Optional[str]) -> Optional[str]:
    pass
def parse_mask(value: Optional[str]) -> Optional[str]:
    pass