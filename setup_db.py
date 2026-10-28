import os
import glob
import duckdb
import pandas as pd

# Nome do arquivo de banco consolidado que será gerado
DB_FILE = "geo_data.duckdb"

# Caminho base onde estão as pastas das análises
BASE_DIR = "Base"

def build_database():
    # Remove o banco antigo se já existir para recriar do zero
    if os.path.exists(DB_FILE):
        os.remove(DB_FILE)

    con = duckdb.connect(DB_FILE)
    print("Iniciando a conversão dos arquivos CSV em tabelas DuckDB...")

    # Busca recursivamente todos os arquivos .csv dentro da pasta Base
    csv_files = glob.glob(f"{BASE_DIR}/**/*.csv", recursive=True)

    for file_path in csv_files:
        # Extrai o nome do arquivo sem extensão para usar como nome da tabela
        file_name = os.path.basename(file_path)
        table_name = os.path.splitext(file_name)[0].lower()
        
        # Limpa o nome da tabela (remove caracteres especiais/espaços)
        table_name = table_name.replace(" ", "_").replace("-", "_")

        print(f"Processando: {file_path} -> Tabela: {table_name}")

        try:
            # Lê o CSV e insere diretamente como uma tabela no DuckDB
            df = pd.read_csv(file_path)
            con.execute(f"CREATE TABLE {table_name} AS SELECT * FROM df")
        except Exception as e:
            print(f"Erro ao processar {file_path}: {e}")

    con.close()
    print(f"\nBanco de dados '{DB_FILE}' criado com sucesso!")

if __name__ == "__main__":
    build_database()