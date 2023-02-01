from reflexgenerator.ingestion.parsers import ColumnParser
from reflexgenerator.ingestion.parsers import (
    parse_converter,
    parse_reg_array_spec,
    parse_address,
    parse_message_type,
    parse_alias,
    parse_name,
    parse_dtype,
    parse_mask_family,

)


_mask2col = [
    ColumnParser(api_name="name", column_name="Name", converter=parse_name, allow_none=False),
    ColumnParser(api_name="alias", column_name="Alias", converter=parse_alias),
    ColumnParser(api_name="mask_family", column_name="MaskFamily", converter=parse_mask_family),
    ColumnParser(api_name="mask", column_name="Mask", allow_none=False),
    ColumnParser(api_name="dtype", column_name="Format", converter=parse_dtype, allow_none=False),
    ColumnParser(api_name="description", column_name="Description", converter=str),
    ColumnParser(api_name="converter", column_name="Converter", converter=parse_converter),
    ColumnParser(api_name="enable_generator", column_name="AutoInterface"),
    ColumnParser(api_name="grouping", column_name="InterfaceGrouping")
    ]

_mask2col = {x.column_name: x for x in _mask2col}

_register2col = [
    ColumnParser(api_name="name", column_name="Name", converter=parse_name, allow_none=False),
    ColumnParser(api_name="address", column_name="Address", converter=parse_address, allow_none=False),
    ColumnParser(api_name="alias", column_name="Alias", converter=parse_alias),
    ColumnParser(api_name="mask_family", column_name="MaskFamily", converter=parse_mask_family),
    ColumnParser(api_name="message_type", column_name="Type", converter=parse_message_type, allow_none=False),
    ColumnParser(api_name="dtype", column_name="Format", converter=parse_dtype, allow_none=False),
    ColumnParser(api_name="array_spec", column_name="PayloadFormat", converter=parse_reg_array_spec),
    ColumnParser(api_name="converter", column_name="Converter", converter=parse_converter),
    ColumnParser(api_name="description", column_name="Description", converter=str),
    ColumnParser(api_name="enable_generator", column_name="AutoInterface"),
    ColumnParser(api_name="grouping", column_name="InterfaceGrouping")
    ]

_register2col = {x.column_name: x for x in _register2col}
