import argparse
import sys
import markdown
from pathlib import Path

import reflexgenerator.sources
from reflexgenerator.sources import DeviceSchema

# python build_docs.py -r "https://raw.githubusercontent.com/harp-tech/device.behavior"
parser = argparse.ArgumentParser()

# Adding CLI arguments
parser.add_argument("-r", "--device_repo",
                    type=str,
                    help="URL for the root of the device repository")
parser.add_argument("-g", "--generate_docs",
                    default=True, type=bool,
                    help="Flag that enables the generation of output files. Default is True.")
parser.add_argument("-o", "--output_path",
                    default=None, type=str,
                    help="Root output path to save the generated files. Default is the current directory.")
parser.add_argument("-n", "--doc_output_name",
                    default="documentation", type=str,
                    help="File name of the generated documentation files (.md and .html). Default is 'docs'.")


# Read arguments from command line

try:
    args = parser.parse_args()
except:
    parser.print_help()
    sys.exit(0)


def build(
        device_repo_url: str,
        generate_docs: bool = True,
        root_output_path=None,
        doc_output_name: str = "documentation",
        branch: str = "master") -> str:

    if root_output_path is None:
        root_output_path = Path(__file__).parent
    root_output_path = Path(root_output_path)

    schema = DeviceSchema.from_remote_yml(
        device_url=f"{device_repo_url}/{branch}/device.yml")
    schema_df = schema.to_dataframe()

    txt = f"""

# Device

{schema.metadata.format_dict()}
--------

# Registers

## Summary table
{reflexgenerator.generator.format_table(schema_df.registers)}

## Technical documentation
{"".join([reg.format_dict() for reg in schema.registers])}
--------

# BitMasks

## Summary table
{reflexgenerator.generator.format_table(schema_df.bitMasks)}

## Technical documentation
{"".join([mask.format_dict() for mask in schema.bitMasks])}

# GroupMasks

## Summary table
{reflexgenerator.generator.format_table(schema_df.groupMasks)}

## Technical documentation
{"".join([mask.format_dict() for mask in schema.groupMasks])}

--------
# PayloadMembers

## Summary table
{reflexgenerator.generator.format_table(schema_df.payloadMembers)}

## Technical documentation
{"".join([payloadMember.format_dict() for payloadMember in schema.payloadMembers])}


## References
{reflexgenerator.sources.PayloadType.format_anchor_references()}

{reflexgenerator.sources.AccessType.format_anchor_references()}

{reflexgenerator.sources.VisibilityType.format_anchor_references()}

{reflexgenerator.sources.MaskCategory.format_anchor_references()}

"""

    if generate_docs is True:
        with open(root_output_path / f"{doc_output_name}.md", "w") as text_file:
            text_file.write(txt)
        with open(root_output_path / f"{doc_output_name}.html", "w") as text_file:
            text_file.write(markdown.markdown(
                txt, tab_length=4,
                extensions=['extra', 'smarty', 'sane_lists']))

    return txt


txt = build(
    device_repo_url=args.device_repo,
    generate_docs=args.generate_docs,
    root_output_path=args.output_path,
    doc_output_name=args.doc_output_name)