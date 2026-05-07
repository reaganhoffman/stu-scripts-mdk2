#!/usr/bin/env python3

import argparse
import csv
import os
import sys
import xml.etree.ElementTree as ET
from collections import Counter


XSI_NAMESPACE = "http://www.w3.org/2001/XMLSchema-instance"


def strip_namespace(tag):
    """
    Converts '{namespace}TagName' to 'TagName'.
    Leaves 'TagName' unchanged if there is no namespace.
    """
    if "}" in tag:
        return tag.split("}", 1)[1]
    return tag


def find_child_text(element, child_name):
    """
    Finds the first direct child with the given local tag name,
    ignoring XML namespaces.
    """
    for child in element:
        if strip_namespace(child.tag) == child_name:
            return child.text or ""
    return ""


def get_block_definition_id(block):
    """
    Returns a definition ID like:

        CubeBlock/LargeBlockArmorBlock
        Reactor/LargeBlockLargeGenerator
        CargoContainer/LargeBlockLargeContainer

    Space Engineers blueprint block nodes often look like:

        <MyObjectBuilder_CubeBlock xsi:type="MyObjectBuilder_Reactor">
            <SubtypeName>LargeBlockLargeGenerator</SubtypeName>
            ...
        </MyObjectBuilder_CubeBlock>
    """

    xsi_type_key = "{" + XSI_NAMESPACE + "}type"

    xsi_type = block.attrib.get(xsi_type_key, "")
    if xsi_type:
        type_id = xsi_type.replace("MyObjectBuilder_", "", 1)
    else:
        type_id = strip_namespace(block.tag).replace("MyObjectBuilder_", "", 1)

    subtype_id = find_child_text(block, "SubtypeName")

    return type_id, subtype_id, f"{type_id}/{subtype_id}"


def find_cube_blocks(root):
    """
    Finds all immediate child blocks under any CubeBlocks node.
    Namespace-agnostic.
    """
    blocks = []

    for element in root.iter():
        if strip_namespace(element.tag) == "CubeBlocks":
            for child in element:
                blocks.append(child)

    return blocks


def export_blueprint_blocks(blueprint_path, output_csv):
    if os.path.isdir(blueprint_path):
        blueprint_path = os.path.join(blueprint_path, "bp.sbc")

    if not os.path.exists(blueprint_path):
        raise FileNotFoundError(f"Blueprint file not found: {blueprint_path}")

    tree = ET.parse(blueprint_path)
    root = tree.getroot()

    blocks = find_cube_blocks(root)

    if not blocks:
        raise ValueError(
            "No blocks found. Make sure you passed either a blueprint folder "
            "or the path to its bp.sbc file."
        )

    counts = Counter()

    for block in blocks:
        type_id, subtype_id, definition_id = get_block_definition_id(block)
        counts[(definition_id, type_id, subtype_id)] += 1

    rows = sorted(
        [
            {
                "DefinitionId": definition_id,
                "TypeId": type_id,
                "SubtypeId": subtype_id,
                "Count": count,
            }
            for (definition_id, type_id, subtype_id), count in counts.items()
        ],
        key=lambda row: row["DefinitionId"],
    )

    with open(output_csv, "w", newline="", encoding="utf-8") as file:
        writer = csv.DictWriter(
            file,
            fieldnames=["DefinitionId", "TypeId", "SubtypeId", "Count"],
        )
        writer.writeheader()
        writer.writerows(rows)

    return len(blocks), len(rows)


def main():
    parser = argparse.ArgumentParser(
        description="Export Space Engineers blueprint block counts to CSV."
    )

    parser.add_argument(
        "blueprint",
        help="Path to a blueprint folder or directly to bp.sbc",
    )

    parser.add_argument(
        "-o",
        "--output",
        default="block-counts.csv",
        help="Output CSV path. Default: block-counts.csv",
    )

    args = parser.parse_args()

    try:
        total_blocks, unique_blocks = export_blueprint_blocks(
            args.blueprint,
            args.output,
        )
    except Exception as error:
        print(f"Error: {error}", file=sys.stderr)
        sys.exit(1)

    print(f"Read {total_blocks} total blocks.")
    print(f"Exported {unique_blocks} unique block definitions to {args.output}.")


if __name__ == "__main__":
    main()