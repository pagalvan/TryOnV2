import fs from "fs-extra"
import path from "path"
import { fileURLToPath } from "url"

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

const sourceDir = path.join(__dirname, "dist")
const targetDir = path.join(__dirname, "../TryOn.API/wwwroot")

async function copyFiles() {
  try {
    console.log("🔄 Iniciando copia de archivos...")
    console.log(`📁 Origen: ${sourceDir}`)
    console.log(`📁 Destino: ${targetDir}`)

    // Verificar que existe el directorio de origen
    if (!(await fs.pathExists(sourceDir))) {
      throw new Error(`El directorio de origen no existe: ${sourceDir}`)
    }

    // Crear directorio de destino si no existe
    await fs.ensureDir(targetDir)

    // Limpiar directorio de destino
    await fs.emptyDir(targetDir)
    console.log("🧹 Directorio de destino limpiado")

    // Copiar archivos construidos
    await fs.copy(sourceDir, targetDir)
    console.log("✅ Archivos copiados exitosamente a wwwroot")

    // Listar archivos copiados
    const files = await fs.readdir(targetDir)
    console.log("📋 Archivos copiados:", files)
  } catch (error) {
    console.error("❌ Error copiando archivos:", error.message)
    process.exit(1)
  }
}

copyFiles()
