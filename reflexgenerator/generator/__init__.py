import pandas as pd
import markdown
import reflexgenerator

from typing import Optional

def export_md_html(
    md_text: str,
    out_filename: str = "documentation.md",
    out_filename_html: Optional[str] = None
    ) -> None:

    with open(out_filename, "w") as text_file:
        text_file.write(md_text)

    if out_filename_html is None:
        out_filename_html = out_filename.replace(".md", ".html")
    with open(out_filename_html, "w") as text_file:
        text_file.write(markdown.markdown(md_text, tab_length=4, extensions=['extra', 'smarty', 'sane_lists']))


def format_table(df: pd.DataFrame) -> str:
    return df.loc[:, df.columns != "uid"].to_markdown(index=False, tablefmt="pipe")


def format_md(schema) -> str:
    df = schema.to_dataframe()

    txt = f"""

# Device

{schema.metadata.format_dict()}
--------

# Registers

## Summary table
{reflexgenerator.generator.format_table(df.registers)}

## Technical documentation
{"".join([reg.format_dict() for reg in schema.registers])}
--------

# BitMasks

## Summary table
{reflexgenerator.generator.format_table(df.bitMasks)}

## Technical documentation
{"".join([mask.format_dict() for mask in schema.bitMasks])}

# GroupMasks

## Summary table
{reflexgenerator.generator.format_table(df.groupMasks)}

## Technical documentation
{"".join([mask.format_dict() for mask in schema.groupMasks])}

--------
# PayloadMembers

## Summary table
{reflexgenerator.generator.format_table(df.payloadMembers)}

## Technical documentation
{"".join([payloadMember.format_dict() for payloadMember in schema.payloadMembers])}


## References
{reflexgenerator.sources.PayloadType.format_anchor_references()}

{reflexgenerator.sources.AccessType.format_anchor_references()}

{reflexgenerator.sources.VisibilityType.format_anchor_references()}

{reflexgenerator.sources.MaskCategory.format_anchor_references()}

"""

    return txt