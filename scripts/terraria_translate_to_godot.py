import json
import csv
import os
import re
from typing import Dict, List

def strip_trailing_commas(json_str):
    pattern = r',(\s*[\}\]])'
    cleaned_str = re.sub(pattern, r'\1', json_str)
    return cleaned_str

def convert_dict_to_list(d: Dict, key: str) -> List:
    l = []
    for k, v in d.items():
        name = f"{key}.{k}"
        if isinstance(v, dict):
            l.extend(convert_dict_to_list(v, name))
        else:
            l.append([name, v])
    return l

def convert_json_to_godot_csv(input_file) -> List:
    with open(input_file, 'r', encoding="utf-8-sig") as f:
        cleaned_str = strip_trailing_commas(f.read())
        data: Dict = json.loads(cleaned_str)
    return convert_dict_to_list(data, os.path.splitext(os.path.basename(input_file))[0])


def save_csv(data, lang, output_file):
    with open(output_file, 'w', newline='', encoding='utf-8') as f:
        writer = csv.writer(f)
        writer.writerow(["KEYS", lang])
        writer.writerows(data)

def convert_all(root_dir: str):
    os.chdir(root_dir)
    langs = os.listdir(".")
    for i in langs:
        if "csv_convert" in i:
            continue
        lang_all_trans = []
        for (root, _, files) in os.walk(i):
            for file in files:
                if file.endswith(".json"):
                    path = os.path.join(root, file)
                    print(f"Converting {path}...")
                    res = convert_json_to_godot_csv(path)
                    lang_all_trans.extend(res)
        sorted(lang_all_trans, key=lambda x: x[0])
        save_csv(lang_all_trans, i, os.path.join("csv_convert", f"{i}.csv"))
    os.chdir("..")


def main():
    os.makedirs("localization/csv_convert", exist_ok=True)
    convert_all("localization")

if __name__ == "__main__":
    main()
