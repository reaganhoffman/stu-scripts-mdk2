import argparse
import os
import xml.etree.ElementTree as ET

from dotenv import load_dotenv

load_dotenv()

entity_id = os.environ["ENTITY_ID"]

if not entity_id:
  raise Exception("Must set an ENTITY_ID in your .env")

parser = argparse.ArgumentParser(description="Retrieves the Storage string of a given database Programmable Block and returns the data as a .csv")
parser.add_argument("location", choices=["local", "remote"])
args = parser.parse_args()

location = args.location

def create_psv(storage):
  with open("./output.csv", "w", newline="", encoding="utf-8") as f:
    f.write(storage)

def get_xml(path):
  tree = ET.parse(path)
  root = tree.getroot()
  node = root.find(f".//MyObjectBuilder_CubeBlock[EntityId='{entity_id}']")
  storage = node.findtext("Storage")
  if not storage:
    raise Exception("Storage string was empty!")
  return storage

def handle_local():
  local_path = os.environ["LOCAL_PATH"]
  if not local_path:
    raise Exception("Local path not set in .env!")
  storage = get_xml(local_path)
  create_psv(storage)

def handle_remote():
  print()

handler = {
  "local": handle_local,
  "remote": handle_remote
}

handler[location]()
