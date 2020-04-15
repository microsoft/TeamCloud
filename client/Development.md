# Development

The Azure CLI TeamCloud extension was developed using [Microsoft Azure CLI Dev Tools (azdev)](https://github.com/Azure/azure-cli-dev-tools).

This document explains how to get a local development environment set up.

## Setting up your development environment

1. Install Python 3.6+ from http://python.org. Please note that the version of Python that comes preinstalled on macOS is 2.7.
2. Fork and clone this repository.
3. Create a new virtual environment for Python in the _client_ (this) directory of your clone. You can do this by running:

    ```sh
    cd /path/to/TeamCloud/client
    ```

    Then:

    Python 3.6+ (all platforms):

    ```sh
    python -m venv env
    ```

    or

    ```sh
    python3 -m venv env
    ```

4. Activate the env virtual environment by running:

    Windows CMD.exe:

    ```BatchFile
    env\Scripts\activate.bat
    ```

    Windows Powershell:

    ```BatchFile
    env\Scripts\activate.ps1
    ```

    OSX/Linux (bash):

    ```sh
    source env/bin/activate
    ```

5. Install `azdev` by running:

    ```sh
    pip install azdev
    ```

6. Complete setup by running:

    ```sh
    azdev setup -r /path/to/TeamCloud
    ```
