from __future__ import annotations
from typing import Optional


DEVICE = "Device"


class UidReference:

    REFERENCES = {}  # internal

    def __init__(
            self,
            parent,
            add_mod_uid_str=None) -> None:

        self._parent = parent
        self._uid = self._generate_uid(add_mod_uid_str)
        self._pointers = []

    @property
    def parent(self):
        return self._parent

    @property
    def uid(self) -> str:
        return self._uid

    @property
    def current_pointers(self) -> list:
        return self._pointers

    def _generate_uid(self, add_mod_uid_str=None) -> str:
        if add_mod_uid_str is None:
            _ref = f"ref-{DEVICE}-{self.parent.__class__.__name__}-{self.parent.name}"
        else:
            _ref = f"ref-{DEVICE}-{self.parent.__class__.__name__}-{add_mod_uid_str}.{self.parent.name}"
        if _ref not in self.REFERENCES:
            self.REFERENCES[_ref] = self
        else:
            raise KeyError(f"A key ({_ref}) with the same name as the current\
                            reference already exists!")
        return _ref

    def render_reference(self,
                         rendered_string: Optional[str] = None
                         ) -> str:
        if rendered_string is None:
            rendered_string = self.parent.name
        return self.make_anchor(self.uid, rendered_string)

    def render_pointer(self,
                       rendered_string: Optional[str] = None
                       ) -> str:
        if rendered_string is None:
            rendered_string = self.parent.name
        _link = self.create_link(self.uid, rendered_string)
        self._pointers.append(_link)
        return _link

    def __repr__(self) -> str:
        return f"{self.uid}"

    def __str__(self) -> str:
        return self.__repr__()

    def reset(self) -> None:
        self.REFERENCES = {}

    @classmethod
    def filter_refs_by_type(self, instance_type: str) -> dict:
        """Filters the references dictionary by the type of the parent object.

        Args:
            type (str): type of the parent object.

        Returns:
            dict: filtered dictionary.
        """
        return {k: v for k, v in UidReference.REFERENCES.items()
                if isinstance(v.parent, instance_type)}

    @classmethod
    def make_anchor(self,
                    reference: str,
                    rendered_string: Optional[str] = None) -> str:
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

    @classmethod
    def create_link(self,
                    reference: str,
                    rendered_string: Optional[str] = None) -> str:
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


