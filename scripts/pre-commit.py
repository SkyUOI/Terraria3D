import os


def main():
    if os.system("uv run gdformat src"):
        print("It seems that uv or gdtoolkit is not installed. Ignore.")
    if os.system("dotnet format Terraria3D.sln --exclude addons"):
        print("It seems that dotnet sdk is not installed. Ignore.")
    if os.system("ruff format"):
        print("It seems that ruff is not installed. Ignore.")


if __name__ == "__main__":
    main()
