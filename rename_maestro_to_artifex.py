#!/usr/bin/env python3
"""
Script to rename everything from Maestro to Artifex
This script will:
1. Rename all directories containing 'maestro' to 'artifex'
2. Rename all files containing 'maestro' to 'artifex'
3. Replace all text content 'Maestro' with 'Artifex' and 'maestro' with 'artifex'
"""

import os
import shutil
from pathlib import Path
import re

PROJECT_ROOT = Path(__file__).parent
EXCLUDE_DIRS = {'.git', 'node_modules', 'bin', 'obj', '__pycache__', '.vs', '.vscode'}
EXCLUDE_FILES = {'rename_maestro_to_artifex.py'}

def should_process(path):
    """Check if path should be processed (not in excluded directories)"""
    parts = path.parts
    return not any(excluded in parts for excluded in EXCLUDE_DIRS)

def rename_in_content(file_path):
    """Replace Maestro/maestro with Artifex/artifex in file content"""
    try:
        with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()

        # Replace all variations
        new_content = content
        new_content = new_content.replace('Maestro', 'Artifex')
        new_content = new_content.replace('maestro', 'artifex')
        new_content = new_content.replace('MAESTRO', 'ARTIFEX')

        if new_content != content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            print(f"✓ Updated content in: {file_path.relative_to(PROJECT_ROOT)}")
            return True
    except Exception as e:
        print(f"✗ Error updating {file_path}: {e}")
    return False

def rename_file(file_path):
    """Rename a file if it contains 'maestro' in its name"""
    if 'maestro' not in file_path.name.lower():
        return file_path

    new_name = file_path.name.replace('Maestro', 'Artifex').replace('maestro', 'artifex')
    new_path = file_path.parent / new_name

    try:
        shutil.move(str(file_path), str(new_path))
        print(f"✓ Renamed file: {file_path.name} -> {new_name}")
        return new_path
    except Exception as e:
        print(f"✗ Error renaming file {file_path}: {e}")
        return file_path

def rename_directory(dir_path):
    """Rename a directory if it contains 'maestro' in its name"""
    if 'maestro' not in dir_path.name.lower():
        return dir_path

    new_name = dir_path.name.replace('Maestro', 'Artifex').replace('maestro', 'artifex')
    new_path = dir_path.parent / new_name

    try:
        shutil.move(str(dir_path), str(new_path))
        print(f"✓ Renamed directory: {dir_path.name} -> {new_name}")
        return new_path
    except Exception as e:
        print(f"✗ Error renaming directory {dir_path}: {e}")
        return dir_path

def main():
    print("=" * 80)
    print("Renaming Maestro to Artifex")
    print("=" * 80)

    # Phase 1: Update content in all files
    print("\n[Phase 1] Updating file contents...")
    print("-" * 80)

    for root, dirs, files in os.walk(PROJECT_ROOT):
        # Remove excluded directories from the walk
        dirs[:] = [d for d in dirs if d not in EXCLUDE_DIRS]

        root_path = Path(root)
        if not should_process(root_path):
            continue

        for file in files:
            if file in EXCLUDE_FILES:
                continue

            file_path = root_path / file
            # Process text files (common extensions)
            if file_path.suffix in {'.cs', '.csproj', '.sln', '.json', '.yml', '.yaml',
                                     '.md', '.txt', '.py', '.sh', '.config', '.xml',
                                     '.dockerfile', '.env', '.sql'}:
                rename_in_content(file_path)

    # Phase 2: Rename files (bottom-up to avoid path issues)
    print("\n[Phase 2] Renaming files...")
    print("-" * 80)

    all_files = []
    for root, dirs, files in os.walk(PROJECT_ROOT):
        dirs[:] = [d for d in dirs if d not in EXCLUDE_DIRS]
        root_path = Path(root)
        if not should_process(root_path):
            continue

        for file in files:
            if file in EXCLUDE_FILES:
                continue
            file_path = root_path / file
            all_files.append(file_path)

    for file_path in all_files:
        rename_file(file_path)

    # Phase 3: Rename directories (deepest first to avoid path issues)
    print("\n[Phase 3] Renaming directories...")
    print("-" * 80)

    all_dirs = []
    for root, dirs, files in os.walk(PROJECT_ROOT):
        dirs[:] = [d for d in dirs if d not in EXCLUDE_DIRS]
        root_path = Path(root)
        if not should_process(root_path):
            continue

        for dir_name in dirs:
            dir_path = root_path / dir_name
            all_dirs.append(dir_path)

    # Sort by depth (deepest first)
    all_dirs.sort(key=lambda p: len(p.parts), reverse=True)

    for dir_path in all_dirs:
        if dir_path.exists():  # Check existence as parent might have been renamed
            rename_directory(dir_path)

    print("\n" + "=" * 80)
    print("Renaming complete!")
    print("=" * 80)
    print("\nNext steps:")
    print("1. Review the changes")
    print("2. Run 'dotnet clean' to remove old build artifacts")
    print("3. Run 'dotnet restore' to restore packages")
    print("4. Run 'dotnet build' to verify everything builds correctly")

if __name__ == '__main__':
    main()
