import pandas as pd
import markdown
import argparse
import sys

from typing import Optional
from pathlib import Path

import reflexgenerator.sources
import reflexgenerator.io

from reflexgenerator.sources import(
    Register, Mask, Collection, Metadata, PayloadMember)
from reflexgenerator.generator.xref import filter_refs_by_type

# python build_docs.py -d "example.yml" -p "ios.yml" -o "C:\Users\neurogears\Documents\git\harptech\automatic_code_generation\reflex-generator\demo_docs" 


parser = argparse.ArgumentParser()

# Adding CLI arguments
parser.add_argument("-i", "--input_path",
                    default=None, type=str,
                    help="Root input path to look for the .yml files. Default is the current directory.")
parser.add_argument("-d", "--device_input",
                    default="device.yml", type=str,
                    help="File name of the .yml file with the device specifications. Default is 'device.yml'.")
parser.add_argument("-p", "--io_input",
                    default="ios.yml", type=str,
                    help="File name of the .yml file with the IO specifications. Default is 'io.yml'.")
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
        root_input_path: Optional[str] = None,
        device_input_path: str = "device.yml",
        io_input_path: str = "ios.yml",
        generate_docs: bool = True,
        root_output_path=None,
        doc_output_name="documentation") -> str:

    if root_input_path is None:
        root_input_path = Path(__file__).parent / "schema"
    root_input_path = Path(root_input_path)

    if root_output_path is None:
        root_output_path = Path(__file__).parent
    root_output_path = Path(root_output_path)

    device = reflexgenerator.io.load(root_input_path / device_input_path)
    ios = reflexgenerator.io.load(root_input_path / io_input_path)

    # Load Metadata
    metadata = Metadata(
        **{
        "device": device["device"],
        "whoAmI": device["whoAmI"],
        "architecture": device["architecture"],
        "firmwareVersion": device["firmwareVersion"],
        "hardwareTargets": device["hardwareTargets"]
        })

    #  Load BitMasks and GroupMasks
    if "bitMasks" in device:
        bitMasks = Collection([Mask.from_json(mask) for mask in device["bitMasks"].items() if mask is not None])
        bitMasks_df = pd.DataFrame([mask.to_dict() for mask in bitMasks])
        bitMasks_df["name"] = bitMasks_df["uid"].apply(
            lambda x: (x.render_pointer()))
        bitMasks_df["bits"] = bitMasks_df["bits"].apply(
            lambda x: [bit.value for bit in x if bit is not None] if x is not None else None)
    else:
        bitMasks = None
        bitMasks_df = pd.DataFrame()

    if "groupMask" in device:
        groupMasks = Collection([Mask.from_json(mask) for mask in device["groupMask"].items() if mask is not None])
        groupMasks_df = pd.DataFrame([mask.to_dict() for mask in groupMasks])
        groupMasks_df["name"] = groupMasks_df["uid"].apply(
            lambda x: (x.render_pointer()))
        bitMasks_df["value"] = bitMasks_df["value"].apply(
            lambda x: [bit.value for bit in x if bit is not None] if x is not None else None)
    else:
        groupMasks = None
        groupMasks_df = pd.DataFrame()

    # Load Registers
    if "registers" in device:
        regs = Collection(
            [Register.from_json(reg) for reg in device["registers"].items()
            if reg is not None])
        register_df = pd.DataFrame([reg.to_dict() for reg in regs])
        register_df["name"] = register_df["uid"].apply(
            lambda x: (x.render_pointer())
            )

        for i in register_df.index:
            if register_df.at[i, 'maskType'] is not None:
                if isinstance(
                    register_df.at[i, 'maskType'][0], Mask):
                    register_df.at[i, 'maskType'] = [
                        x.uid.render_pointer() for x in register_df.at[i, 'maskType']]
            if register_df.at[i, 'payloadSpec'] is not None:
                if isinstance(register_df.at[i, 'payloadSpec'][0], PayloadMember):
                    register_df.at[i, 'payloadSpec'] = [
                        x.uid.render_pointer() for x in register_df.at[i, 'payloadSpec']]
    else:
        regs = None
        register_df = pd.DataFrame()

    # Load PayloadMembers
    payloadMembers = Collection([
        entry.parent for entry in filter_refs_by_type(reflexgenerator.sources.PayloadMember).values()])
    if len(payloadMembers.elements) > 0:
        payloadMembers_df = pd.DataFrame([
            payloadMember.to_dict() for payloadMember in payloadMembers])
        payloadMembers_df["name"] = payloadMembers_df["uid"].apply(
            lambda x: (x.render_pointer()))
    else:
        payloadMembers = None
        payloadMembers_df = pd.DataFrame()

    # Build IOs
    pinMapping = Collection([
        reflexgenerator.sources.PinMap_from_json(pinmap) for pinmap in ios.items() if pinmap is not None])
    if len(pinMapping.elements) > 0:
        pinMapping_df = pd.DataFrame([pinmap.to_dict() for pinmap in pinMapping])
        pinMapping_df["name"] = pinMapping_df["uid"].apply(lambda x: (x.render_pointer()))
    else:
        pinMapping = None
        pinMapping_df = pd.DataFrame()

    # Generate the markdown document


    txt = f"""

# Device

{metadata.format_dict()}

--------

# Registers

## Summary table
{reflexgenerator.generator.format_table(register_df)}

## Technical documentation
{"".join([reg.format_dict() for reg in regs])}
--------

# BitMasks

## Summary table
{reflexgenerator.generator.format_table(bitMasks_df)}

## Technical documentation
{"".join([mask.format_dict() for mask in bitMasks])}
--------
# PayloadMembers

## Summary table
{reflexgenerator.generator.format_table(payloadMembers_df)}

## Technical documentation
{"".join([payloadMember.format_dict() for payloadMember in payloadMembers])}

# IOs
{reflexgenerator.generator.format_table(pinMapping_df)}

## Technical documentation
{"".join([pin.format_dict() for pin in pinMapping])}


## References
{reflexgenerator.sources.PayloadType.format_anchor_references()}

{reflexgenerator.sources.RegisterType.format_anchor_references()}

{reflexgenerator.sources.VisibilityType.format_anchor_references()}

{reflexgenerator.sources.MaskCategory.format_anchor_references()}

{reflexgenerator.sources.DirectionType.format_anchor_references()}

{reflexgenerator.sources.InputPinModeType.format_anchor_references()}

{reflexgenerator.sources.TriggerModeType.format_anchor_references()}

{reflexgenerator.sources.InterruptPriorityType.format_anchor_references()}

{reflexgenerator.sources.OutputPinModeType.format_anchor_references()}

{reflexgenerator.sources.InitialStateType.format_anchor_references()}

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
    root_input_path=args.input_path,
    device_input_path=args.device_input,
    io_input_path=args.io_input,
    generate_docs=args.generate_docs,
    root_output_path=args.output_path,
    doc_output_name=args.doc_output_name)