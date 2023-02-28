import pandas as pd


def format_table(df: pd.DataFrame) -> str:
    return df.loc[:, df.columns != "uid"].to_markdown(index=False, tablefmt="pipe")