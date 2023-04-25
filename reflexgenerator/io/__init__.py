import yaml
from yaml import Loader
import requests


def load(path_or_url, from_url=False):
    if not from_url:
        with open(path_or_url, 'r') as stream:
            try:
                return yaml.load(stream, Loader)
            except yaml.YAMLError as exception:
                raise exception
    else:
        response = requests.get(path_or_url, allow_redirects=True)
        content = response.content.decode("utf-8")
        content = yaml.safe_load(content)
        return content
