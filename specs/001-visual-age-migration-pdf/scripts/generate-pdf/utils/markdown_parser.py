#!/usr/bin/env python3
"""
Markdown Parser Utility for Visual Age Migration PDF Generation
Parses markdown files and extracts structured sections for LaTeX conversion
"""

import re
from pathlib import Path
from typing import Dict, List, Tuple, Optional
import markdown2


class MarkdownParser:
    """Parser for extracting and converting markdown content to LaTeX-friendly format"""

    def __init__(self):
        """Initialize the markdown parser"""
        self.md = markdown2.Markdown(
            extras=[
                'tables',
                'fenced-code-blocks',
                'header-ids',
                'toc',
                'footnotes',
                'smarty-pants'
            ]
        )

    def parse_file(self, file_path: str) -> Dict[str, any]:
        """Parse a markdown file and extract structured content"""
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()

        return self.parse_content(content)

    def parse_content(self, content: str) -> Dict[str, any]:
        """Parse markdown content and extract structured sections"""
        # Extract frontmatter if present
        frontmatter = self._extract_frontmatter(content)

        # Remove frontmatter from content
        if frontmatter:
            content = re.sub(r'^---\n.*?\n---\n', '', content, flags=re.DOTALL)

        # Extract sections
        sections = self._extract_sections(content)

        # Extract code blocks
        code_blocks = self._extract_code_blocks(content)

        # Extract tables
        tables = self._extract_tables(content)

        # Extract lists
        lists = self._extract_lists(content)

        return {
            'frontmatter': frontmatter,
            'sections': sections,
            'code_blocks': code_blocks,
            'tables': tables,
            'lists': lists,
            'raw_content': content
        }

    def _extract_frontmatter(self, content: str) -> Optional[Dict]:
        """Extract YAML frontmatter from markdown"""
        pattern = r'^---\n(.*?)\n---'
        match = re.match(pattern, content, re.DOTALL)

        if match:
            import yaml
            try:
                return yaml.safe_load(match.group(1))
            except:
                return None
        return None

    def _extract_sections(self, content: str) -> List[Dict]:
        """Extract hierarchical sections from markdown"""
        sections = []

        # Pattern to match headers
        header_pattern = r'^(#{1,6})\s+(.*?)$'

        lines = content.split('\n')
        current_section = None
        section_content = []

        for line in lines:
            header_match = re.match(header_pattern, line)

            if header_match:
                # Save previous section if exists
                if current_section:
                    current_section['content'] = '\n'.join(section_content).strip()
                    sections.append(current_section)

                # Start new section
                level = len(header_match.group(1))
                title = header_match.group(2)
                current_section = {
                    'level': level,
                    'title': title,
                    'id': self._generate_section_id(title),
                    'content': ''
                }
                section_content = []
            elif current_section:
                section_content.append(line)

        # Save last section
        if current_section:
            current_section['content'] = '\n'.join(section_content).strip()
            sections.append(current_section)

        return sections

    def _extract_code_blocks(self, content: str) -> List[Dict]:
        """Extract code blocks from markdown"""
        code_blocks = []

        # Fenced code blocks
        fenced_pattern = r'```(\w*)\n(.*?)\n```'
        matches = re.findall(fenced_pattern, content, re.DOTALL)

        for language, code in matches:
            code_blocks.append({
                'language': language or 'text',
                'code': code.strip()
            })

        # Indented code blocks
        indented_pattern = r'\n\n((?:    .*\n)+)'
        matches = re.findall(indented_pattern, content)

        for code in matches:
            # Remove indentation
            cleaned_code = '\n'.join(line[4:] for line in code.split('\n') if line.strip())
            code_blocks.append({
                'language': 'text',
                'code': cleaned_code
            })

        return code_blocks

    def _extract_tables(self, content: str) -> List[Dict]:
        """Extract markdown tables"""
        tables = []

        # Simple table pattern
        table_pattern = r'(\|.*\|.*\n)+(\|[-:\s|]+\n)(\|.*\|.*\n)+'
        matches = re.findall(table_pattern, content)

        for match in matches:
            table_text = ''.join(match)
            lines = table_text.strip().split('\n')

            if len(lines) >= 3:  # Header, separator, at least one row
                # Parse header
                header = [cell.strip() for cell in lines[0].split('|')[1:-1]]

                # Parse alignment from separator
                separator = lines[1].split('|')[1:-1]
                alignment = []
                for sep in separator:
                    sep = sep.strip()
                    if sep.startswith(':') and sep.endswith(':'):
                        alignment.append('center')
                    elif sep.endswith(':'):
                        alignment.append('right')
                    else:
                        alignment.append('left')

                # Parse rows
                rows = []
                for line in lines[2:]:
                    row = [cell.strip() for cell in line.split('|')[1:-1]]
                    rows.append(row)

                tables.append({
                    'header': header,
                    'alignment': alignment,
                    'rows': rows
                })

        return tables

    def _extract_lists(self, content: str) -> List[Dict]:
        """Extract ordered and unordered lists"""
        lists = []

        # Unordered list pattern
        ul_pattern = r'(?:^|\n)((?:[-*+]\s+.*(?:\n|$))+)'
        matches = re.findall(ul_pattern, content)

        for match in matches:
            items = []
            for line in match.strip().split('\n'):
                if re.match(r'^[-*+]\s+', line):
                    items.append(re.sub(r'^[-*+]\s+', '', line))

            if items:
                lists.append({
                    'type': 'unordered',
                    'items': items
                })

        # Ordered list pattern
        ol_pattern = r'(?:^|\n)((?:\d+\.\s+.*(?:\n|$))+)'
        matches = re.findall(ol_pattern, content)

        for match in matches:
            items = []
            for line in match.strip().split('\n'):
                if re.match(r'^\d+\.\s+', line):
                    items.append(re.sub(r'^\d+\.\s+', '', line))

            if items:
                lists.append({
                    'type': 'ordered',
                    'items': items
                })

        return lists

    def _generate_section_id(self, title: str) -> str:
        """Generate a valid section ID from title"""
        # Remove special characters and convert to lowercase
        section_id = re.sub(r'[^\w\s-]', '', title.lower())
        # Replace spaces with hyphens
        section_id = re.sub(r'[-\s]+', '-', section_id)
        return section_id.strip('-')

    def markdown_to_latex(self, content: str) -> str:
        """Convert markdown content to LaTeX format"""
        latex = content

        # Escape LaTeX special characters
        latex = self.escape_latex(latex)

        # Convert headers
        latex = re.sub(r'^# (.*?)$', r'\\section{\1}', latex, flags=re.MULTILINE)
        latex = re.sub(r'^## (.*?)$', r'\\subsection{\1}', latex, flags=re.MULTILINE)
        latex = re.sub(r'^### (.*?)$', r'\\subsubsection{\1}', latex, flags=re.MULTILINE)
        latex = re.sub(r'^#### (.*?)$', r'\\paragraph{\1}', latex, flags=re.MULTILINE)

        # Convert emphasis
        latex = re.sub(r'\*\*\*(.*?)\*\*\*', r'\\textbf{\\textit{\1}}', latex)
        latex = re.sub(r'\*\*(.*?)\*\*', r'\\textbf{\1}', latex)
        latex = re.sub(r'\*(.*?)\*', r'\\textit{\1}', latex)
        latex = re.sub(r'`(.*?)`', r'\\texttt{\1}', latex)

        # Convert lists
        latex = self._convert_lists_to_latex(latex)

        # Convert links
        latex = re.sub(r'\[([^\]]+)\]\(([^)]+)\)', r'\\href{\2}{\1}', latex)

        # Convert code blocks
        latex = self._convert_code_blocks_to_latex(latex)

        return latex

    def escape_latex(self, text: str) -> str:
        """Escape special LaTeX characters"""
        # Order matters for replacement
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

    def _convert_lists_to_latex(self, content: str) -> str:
        """Convert markdown lists to LaTeX format"""
        # Convert unordered lists
        ul_pattern = r'(?:^|\n)((?:[-*+]\s+.*(?:\n|$))+)'

        def replace_ul(match):
            items = match.group(1).strip().split('\n')
            latex_items = []
            for item in items:
                item_text = re.sub(r'^[-*+]\s+', '', item)
                latex_items.append(f'  \\item {item_text}')

            return '\n\\begin{itemize}\n' + '\n'.join(latex_items) + '\n\\end{itemize}\n'

        content = re.sub(ul_pattern, replace_ul, content)

        # Convert ordered lists
        ol_pattern = r'(?:^|\n)((?:\d+\.\s+.*(?:\n|$))+)'

        def replace_ol(match):
            items = match.group(1).strip().split('\n')
            latex_items = []
            for item in items:
                item_text = re.sub(r'^\d+\.\s+', '', item)
                latex_items.append(f'  \\item {item_text}')

            return '\n\\begin{enumerate}\n' + '\n'.join(latex_items) + '\n\\end{enumerate}\n'

        content = re.sub(ol_pattern, replace_ol, content)

        return content

    def _convert_code_blocks_to_latex(self, content: str) -> str:
        """Convert code blocks to LaTeX listings"""
        # Fenced code blocks
        fenced_pattern = r'```(\w*)\n(.*?)\n```'

        def replace_fenced(match):
            language = match.group(1) or 'text'
            code = match.group(2)

            return f'''\\begin{{lstlisting}}[language={language}]
{code}
\\end{{lstlisting}}'''

        content = re.sub(fenced_pattern, replace_fenced, content, flags=re.DOTALL)

        return content

    def table_to_latex(self, table: Dict) -> str:
        """Convert a parsed table to LaTeX format"""
        header = table['header']
        rows = table['rows']
        alignment = table['alignment']

        # Convert alignment to LaTeX format
        col_spec = ''.join(['l' if a == 'left' else 'r' if a == 'right' else 'c' for a in alignment])

        latex = f"\\begin{{tabular}}{{{col_spec}}}\n"
        latex += "\\toprule\n"

        # Header
        latex += ' & '.join(header) + ' \\\\\n'
        latex += "\\midrule\n"

        # Rows
        for row in rows:
            latex += ' & '.join(row) + ' \\\\\n'

        latex += "\\bottomrule\n"
        latex += "\\end{tabular}\n"

        return latex


def main():
    """Main function for testing the markdown parser"""
    import argparse
    import json

    parser = argparse.ArgumentParser(description='Parse markdown files for LaTeX conversion')
    parser.add_argument('file', help='Markdown file to parse')
    parser.add_argument('--output', '-o', help='Output JSON file')
    parser.add_argument('--latex', '-l', action='store_true', help='Convert to LaTeX')

    args = parser.parse_args()

    md_parser = MarkdownParser()

    if args.latex:
        with open(args.file, 'r', encoding='utf-8') as f:
            content = f.read()
        latex = md_parser.markdown_to_latex(content)
        print(latex)
    else:
        parsed = md_parser.parse_file(args.file)

        if args.output:
            with open(args.output, 'w', encoding='utf-8') as f:
                json.dump(parsed, f, indent=2, ensure_ascii=False)
            print(f"Parsed content saved to {args.output}")
        else:
            print(json.dumps(parsed, indent=2, ensure_ascii=False))


if __name__ == '__main__':
    main()