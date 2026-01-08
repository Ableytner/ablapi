# ablapi

Ableytners' general-purpose Web API (Application Programming Interface).

## Overview

This project is a collection of many small helper modules that can be used for all kinds of projects.

It is structured into small submodules, which are all optional and not dependent on each other. Feel free to only use the ones you need.

The following submodules are available:
1. Algorithms (`abllib.alg`)
2. Errors (`abllib.error`)
3. File system operations (`abllib.fs`)
4. Fuzzy matching (`abllib.fuzzy`)
5. General (`abllib.general`)
6. Logging (`abllib.log`)
7. Cleanup on exit (`abllib.onexit`)
8. Parallel processing (`abllib.pproc`)
9. Storages (`abllib.storage`)
10. Function wrappers (`abllib.wrapper`)

## Installation

### Github

To install the latest development version directly from Github, run the following command:
```bash
pip install git+https://github.com/Ableytner/abllib
```

Additionally, a [wheel](https://peps.python.org/pep-0427/) is added to every [stable release](https://github.com/Ableytner/abllib/releases), which can be manually downloaded and installed.

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
