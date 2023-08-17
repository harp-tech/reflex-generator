from mergedeep import merge
import reflexgenerator.sources
from reflexgenerator.sources import (
    Register,
    Mask,
    PinMap_from_json,
    Metadata,
    Collection,
    DeviceSchema)
from reflexgenerator.io import load

from pathlib import Path
import pandas as pd
from reflexgenerator.generator import export_md_html, format_md
from reflexgenerator.generator.xref import UidReference


def generate_docs(device_list):
    schemas = {}
    dataframes = {}
    references = {}
    for device in device_list:
        print(device)
        devname = device.split(".")[1]
        schemas[devname] = DeviceSchema.from_remote_yml(device_url=f"https://raw.githubusercontent.com/harp-tech/{device}/master/device.yml")
        dataframes[devname] = schemas[devname].to_dataframe()
        export_md_html(format_md(schemas[devname]), f"demo_docs\{devname}.md")
        references[devname] = UidReference.pop_references()


if __name__ == "__main__":
    devices = [
        'device.outputexpander',
        'device.syringepump',
        'device.timestampgeneratorgen3',
        'device.cameracontrollergen2',
        'device.soundcard',
        'device.synchronizer',
        'device.loadcells',
        'device.audioswitch',
        'device.behavior',
        'device.analoginput',
        'device.rfidreader',
        ]
    generate_docs(devices)