import os, glob

files = glob.glob('Tool/balance/scratch_*.py')
deleted_count = 0
for f in files:
    try:
        os.remove(f)
        deleted_count += 1
    except Exception as e:
        print(f"Error removing {f}: {e}")

print(f"Cleaned up {deleted_count} scratch files in Tool/balance/")
