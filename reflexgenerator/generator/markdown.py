from typing import Optional


class AnchorReference:
    REFERENCES = {}  # internal

    def __init__(
            self,
            reference: str,
            rendered_string: Optional[str] = None,
            parent: Optional[object] = None) -> None:

        self._reference = self.reference = reference
        self.label = self.rendered_string = rendered_string
        self._pointers = []
        self._add_reference()
        self.parent = parent


    @property
    def reference(self) -> str:
        return self._reference

    @reference.setter
    def reference(self, value: str):
        self._reference = value

    @property
    def rendered_string(self) -> str:
        if self.label is None:
            return self._reference
        else:
            return self.label

    @rendered_string.setter
    def rendered_string(self, value: Optional[str]):
        if value is None:
            self.label = self._reference
        else:
            self.label = value

    def render_reference(self,
                         rendered_string: Optional[str] = None
                         ) -> str:
        if rendered_string is None:
            rendered_string = self.rendered_string
        return make_anchor(self.reference, rendered_string)

    def render_pointer(self, rendered_string: Optional[str] = None) -> str:
        if rendered_string is None:
            rendered_string = self.rendered_string
        _link = create_link(self.reference, rendered_string)
        self._pointers.append(_link)
        return _link

    def __repr__(self) -> str:
        return f"Reference to {self.reference}"

    def __str__(self) -> str:
        return self.__repr__()

    def _add_reference(self) -> None:
        if self.reference in self.REFERENCES.keys():
            raise KeyError("A key with the same name as\
                            the current reference already exists!")
        else:
            self.REFERENCES[self.reference] = self


def make_anchor(reference: str, rendered_string: Optional[str] = None) -> str:
    """Outputs a string with the format necessary to create
    a markdown reference anchor.

    Args:
        reference (str): string used to create the reference tag.
        rendered_string (str): string that will be shown.
        Defaults to reference if left None.

    Returns:
        str: Formatted string.
    """
    rendered_string = reference if rendered_string is None else rendered_string
    return f'<a name="{reference}"></a>{rendered_string}'


def create_link(reference: str, rendered_string: Optional[str] = None) -> str:
    """Creates a jump point to a markdown anchor.

    Args:
        reference (str): string used to create the reference tag.
        rendered_string (str): string that will be shown.
        Defaults to reference if left None.

    Returns:
        str: Formatted string.
    """
    rendered_string = reference if rendered_string is None else rendered_string
    return f'[{rendered_string}](#{reference})'