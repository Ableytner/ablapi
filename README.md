# ablapi

Ableytners' general-purpose REST API.

## Overview

This project is a collection of small API (Application Programming Interface) endpoints serving various uses.

It is structured into small submodules, which are all optional and not dependent on each other. Feel free to only use the ones you need.

Each submodule is defined by one python file, which is only loaded if specified in the config. The module may be in a subfolder.

The following submodules are available:
1. hello_world.py (`ablapi.hello_world`)
1. gtnh.py (`ablapi.gtnh`)

## Installation

### Github container registry

TODO

## Development environment setup

If you want to contribute to this project, you need to set up your local environment.

### Clone the repository

Run the command
```bash
git clone https://github.com/Ableytner/ablapi
cd ablapi
```
in your terminal.

### Install pip packages

To install all optional as well as development python packages, run the following commands in the project root.

Windows:
```bash
py -m venv venv
venv\Scripts\activate.bat
pip install -r requirements.txt
```

Linux:
```bash
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
```

### Git pre-commit hooks

Pre-commit hooks are used to check and autofix formatting issues and typos before you commit your changes.
Once installed, they run automatically if you run `git commit ...`.

Using these is optional, but encouraged.

```bash
pip install pre-commit
pre-commit install
```

To verify the installation and run all checks:
```bash
pre-commit run --all-files
```
