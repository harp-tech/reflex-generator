import yaml
from yaml import Loader


def load(yml_file):
    with open(yml_file, 'r') as stream:
        try:
            return yaml.load(stream, Loader)
        except yaml.YAMLError as exception:
            raise exception