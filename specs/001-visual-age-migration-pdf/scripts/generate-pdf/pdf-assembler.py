#!/usr/bin/env python3
"""PDF assembler for LaTeX compilation"""

import subprocess
import shutil
from pathlib import Path

class PDFAssembler:
    def __init__(self, output_dir: str):
        self.output_dir = Path(output_dir)

    def compile_latex(self, tex_file: str, passes: int = 3) -> bool:
        """Compile LaTeX to PDF with multiple passes"""
        for i in range(passes):
            result = subprocess.run(
                ['pdflatex', '-interaction=nonstopmode', '-output-directory',
                 str(self.output_dir), tex_file],
                capture_output=True, text=True
            )
            if result.returncode != 0 and i == passes - 1:
                print(f"LaTeX compilation failed: {result.stderr}")
                return False
        return True

    def clean_auxiliary_files(self):
        """Remove auxiliary LaTeX files"""
        for ext in ['.aux', '.log', '.toc', '.lof', '.lot', '.out']:
            for file in self.output_dir.glob(f'*{ext}'):
                file.unlink()
