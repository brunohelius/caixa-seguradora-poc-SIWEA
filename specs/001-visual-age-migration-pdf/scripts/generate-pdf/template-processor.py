#!/usr/bin/env python3
"""Template processor for LaTeX generation"""

import json
from pathlib import Path
from jinja2 import Environment, FileSystemLoader, select_autoescape

class TemplateProcessor:
    def __init__(self, template_dir: str):
        self.env = Environment(
            loader=FileSystemLoader(template_dir),
            autoescape=select_autoescape(['html', 'xml']),
            block_start_string='<%',
            block_end_string='%>',
            variable_start_string='{{',
            variable_end_string='}}'
        )

    def process(self, template_name: str, context: dict) -> str:
        template = self.env.get_template(template_name)
        return template.render(**context)

    def escape_latex(self, text: str) -> str:
        """Escape special LaTeX characters"""
        replacements = [
            ('\\', '\\textbackslash{}'),
            ('&', '\\&'),
            ('%', '\\%'),
            ('$', '\\$'),
            ('#', '\\#'),
            ('_', '\\_'),
            ('{', '\\{'),
            ('}', '\\}'),
            ('~', '\\textasciitilde{}'),
            ('^', '\\textasciicircum{}'),
        ]
        for old, new in replacements:
            text = text.replace(old, new)
        return text
