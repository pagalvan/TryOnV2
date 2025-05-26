import { bootstrapCameraKit } from "@snap/camera-kit"

// Definir tipos correctos para evitar errores de TypeScript
type ColorType = "blanco" | "negro" | "rojo" | "default"
type CategoriaType = "Camisetas" | "Vestidos" | "Uniformes" | "Pantalones" | "Calzado" | "Accesorios"

// Usar Record en lugar de interface con index signature
type LensMapping = Record<string, string>
type CategoryLenses = Record<CategoriaType, LensMapping>

// Configuración de lentes con tipado correcto
const LENS_CONFIG = {
  groupId: "9cb32233-4f96-4a34-bdb8-b405ffdc21a3",
  lenses: {
    Camisetas: {
      blanco: "732ae023-0a17-4430-a84f-f5c89c59ea9f",
      negro: "52e517f6-79f2-438d-9279-02dd6d46b887",
      rojo: "52e517f6-79f2-438d-9279-02dd6d46b887",
      default: "732ae023-0a17-4430-a84f-f5c89c59ea9f",
    },
    Vestidos: {
      blanco: "45108ebf-72cb-4499-a115-f47dcbcbac26",
      negro: "015a1d10-6e76-4b81-aa84-7b5d40b8b6e9",
      rojo: "015a1d10-6e76-4b81-aa84-7b5d40b8b6e9",
      default: "015a1d10-6e76-4b81-aa84-7b5d40b8b6e9",
    },
    Uniformes: {
      blanco: "63e84884-49eb-4b06-96d1-7da1bebc800b",
      negro: "63e84884-49eb-4b06-96d1-7da1bebc800b",
      rojo: "63e84884-49eb-4b06-96d1-7da1bebc800b",
      default: "63e84884-49eb-4b06-96d1-7da1bebc800b",
    },
    Pantalones: {
      blanco: "52e517f6-79f2-438d-9279-02dd6d46b887",
      negro: "52e517f6-79f2-438d-9279-02dd6d46b887",
      rojo: "52e517f6-79f2-438d-9279-02dd6d46b887",
      default: "52e517f6-79f2-438d-9279-02dd6d46b887",
    },
    Calzado: {
      blanco: "52e517f6-79f2-438d-9279-02dd6d46b887",
      negro: "52e517f6-79f2-438d-9279-02dd6d46b887",
      rojo: "52e517f6-79f2-438d-9279-02dd6d46b887",
      default: "52e517f6-79f2-438d-9279-02dd6d46b887",
    },
    Accesorios: {
      blanco: "52e517f6-79f2-438d-9279-02dd6d46b887",
      negro: "52e517f6-79f2-438d-9279-02dd6d46b887",
      rojo: "52e517f6-79f2-438d-9279-02dd6d46b887",
      default: "52e517f6-79f2-438d-9279-02dd6d46b887",
    },
  } satisfies CategoryLenses,
} as const

// Variables globales con tipado
let cameraKit: any
let session: any
let mediaStream: MediaStream
let selectedProduct: any = null
let selectedColor: ColorType = "blanco"
let currentCategory: CategoriaType = "Camisetas" // NUEVA: Variable para trackear categoría actual

// NUEVA: Función super robusta para detectar categoría
function detectarCategoriaProducto(producto: any): CategoriaType {
  console.log("🔍 DETECTANDO CATEGORÍA PARA:", producto)

  // Método 1: Categoría explícita en el objeto
  if (producto?.categoria?.nombre) {
    const categoriaNormalizada = normalizarCategoria(producto.categoria.nombre)
    console.log(`✅ Categoría encontrada en categoria.nombre: ${categoriaNormalizada}`)
    return categoriaNormalizada
  }

  // Método 2: Categoría directa
  if (producto?.categoria) {
    const categoriaNormalizada = normalizarCategoria(producto.categoria)
    console.log(`✅ Categoría encontrada en categoria: ${categoriaNormalizada}`)
    return categoriaNormalizada
  }

  // Método 3: Detectar por nombre del producto
  if (producto?.nombre) {
    const nombreLower = producto.nombre.toLowerCase()
    console.log(`🔍 Analizando nombre: "${nombreLower}"`)

    // Palabras clave para cada categoría
    const categoriaKeywords = {
      Vestidos: ["vestido", "dress", "vestidos"],
      Uniformes: ["uniforme", "uniform", "uniformes", "escolar", "trabajo"],
      Pantalones: ["pantalon", "pantalones", "jean", "jeans", "pants", "baggy"],
      Calzado: ["zapato", "zapatos", "calzado", "shoe", "shoes", "tennis", "botas"],
      Accesorios: ["accesorio", "accesorios", "collar", "pulsera", "anillo", "gorra", "sombrero"],
      Camisetas: ["camiseta", "camisetas", "playera", "playeras", "t-shirt", "tshirt", "shirt", "blusa"],
    }

    // Buscar coincidencias
    for (const [categoria, keywords] of Object.entries(categoriaKeywords)) {
      for (const keyword of keywords) {
        if (nombreLower.includes(keyword)) {
          console.log(`✅ Categoría detectada por palabra clave "${keyword}": ${categoria}`)
          return categoria as CategoriaType
        }
      }
    }
  }

  // Método 4: Detectar por descripción
  if (producto?.descripcion) {
    const descripcionLower = producto.descripcion.toLowerCase()
    console.log(`🔍 Analizando descripción: "${descripcionLower}"`)

    if (descripcionLower.includes("vestido")) return "Vestidos"
    if (descripcionLower.includes("uniforme")) return "Uniformes"
    if (descripcionLower.includes("pantalon") || descripcionLower.includes("jean")) return "Pantalones"
    if (descripcionLower.includes("zapato") || descripcionLower.includes("calzado")) return "Calzado"
    if (descripcionLower.includes("accesorio")) return "Accesorios"
  }

  console.log("⚠️ No se pudo detectar categoría, usando Camisetas por defecto")
  return "Camisetas"
}

// Función para normalizar categoría con type guard
function normalizarCategoria(categoria: string): CategoriaType {
  const categoriaMap: Record<string, CategoriaType> = {
    // Camisetas
    camisetas: "Camisetas",
    camiseta: "Camisetas",
    playeras: "Camisetas",
    playera: "Camisetas",
    "t-shirt": "Camisetas",
    tshirt: "Camisetas",
    shirt: "Camisetas",
    blusa: "Camisetas",

    // Vestidos
    vestidos: "Vestidos",
    vestido: "Vestidos",
    dress: "Vestidos",

    // Uniformes
    uniformes: "Uniformes",
    uniforme: "Uniformes",
    uniform: "Uniformes",
    escolar: "Uniformes",
    trabajo: "Uniformes",

    // Pantalones
    pantalones: "Pantalones",
    pantalon: "Pantalones",
    jeans: "Pantalones",
    jean: "Pantalones",
    pants: "Pantalones",
    baggy: "Pantalones",

    // Calzado
    calzado: "Calzado",
    zapatos: "Calzado",
    zapato: "Calzado",
    shoes: "Calzado",
    shoe: "Calzado",
    tennis: "Calzado",
    botas: "Calzado",

    // Accesorios
    accesorios: "Accesorios",
    accesorio: "Accesorios",
    accessories: "Accesorios",
    collar: "Accesorios",
    pulsera: "Accesorios",
    anillo: "Accesorios",
    gorra: "Accesorios",
    sombrero: "Accesorios",
  }

  const categoriaLower = categoria.toLowerCase().trim()
  const categoriaMapeada = categoriaMap[categoriaLower]

  return categoriaMapeada || "Camisetas" // Default fallback
}

// Función para normalizar color
function normalizarColor(color: string): ColorType {
  const colorMap: Record<string, ColorType> = {
    blanco: "blanco",
    white: "blanco",
    negro: "negro",
    black: "negro",
    rojo: "rojo",
    red: "rojo",
    azul: "negro", // Mapear azul a negro como fallback
    blue: "negro",
    verde: "negro", // Mapear verde a negro como fallback
    green: "negro",
  }

  const colorLower = color.toLowerCase()
  return colorMap[colorLower] || "blanco" // Default fallback
}

// Función para obtener el lens ID de forma type-safe
function getLensId(categoria: string, color: string): string {
  try {
    const categoriaNormalizada = normalizarCategoria(categoria)
    const colorNormalizado = normalizarColor(color)

    console.log(`🔍 Buscando lente para: ${categoriaNormalizada} - ${colorNormalizado}`)

    // Acceso type-safe a la configuración
    const categoryLenses = LENS_CONFIG.lenses[categoriaNormalizada]

    if (!categoryLenses) {
      console.warn(`⚠️ Categoría no encontrada: ${categoriaNormalizada}`)
      return LENS_CONFIG.lenses.Camisetas.default
    }

    // Acceso seguro al color específico
    const lensId = categoryLenses[colorNormalizado] || categoryLenses.default

    if (!lensId) {
      console.warn(`⚠️ Lente no encontrado para ${categoriaNormalizada}-${colorNormalizado}`)
      return LENS_CONFIG.lenses.Camisetas.default
    }

    console.log(`✅ Lente encontrado: ${lensId}`)
    return lensId
  } catch (error) {
    console.error("❌ Error obteniendo lens ID:", error)
    return LENS_CONFIG.lenses.Camisetas.default
  }
}

// Función para aplicar lente (MEJORADA)
async function applyLens(categoria: string, color: string): Promise<void> {
  if (!cameraKit || !session) {
    console.error("❌ CameraKit o session no están inicializados")
    return
  }

  try {
    const lensId = getLensId(categoria, color)

    console.log(`🎯 Aplicando lente para ${categoria} - ${color}`)
    console.log(`📷 Lens ID: ${lensId}`)
    console.log(`🆔 Group ID: ${LENS_CONFIG.groupId}`)

    // Mostrar loading en la UI
    showLensNotification("Cambiando lente...", "loading")

    // Cargar el nuevo lente
    const lens = await cameraKit.lensRepository.loadLens(lensId, LENS_CONFIG.groupId)

    // Aplicar el lente
    await session.applyLens(lens)

    console.log("✅ Lente aplicado exitosamente")

    // Mostrar notificación de éxito
    showLensNotification(`Lente aplicado: ${categoria} ${color}`, "success")
  } catch (error) {
    console.error("❌ Error aplicando lente:", error)
    showLensNotification("Error al cambiar el lente", "error")
  }
}

// Función para remover lente
async function removeLens(): Promise<void> {
  if (!session) {
    console.warn("⚠️ Session no disponible para remover lente")
    return
  }

  try {
    await session.clearLens()
    console.log("🧹 Lente removido")
    showLensNotification("Lente removido", "success")
  } catch (error) {
    console.error("❌ Error removiendo lente:", error)
    showLensNotification("Error al remover lente", "error")
  }
}

// NUEVA: Función para forzar cambio de categoría
async function forceChangeCategory(newCategory: CategoriaType): Promise<void> {
  console.log(`🔄 FORZANDO CAMBIO DE CATEGORÍA A: ${newCategory}`)
  currentCategory = newCategory
  await applyLens(newCategory, selectedColor)
}

// Función para cambiar color (COMPLETAMENTE REESCRITA)
async function changeColor(newColor: string): Promise<void> {
  const colorNormalizado = normalizarColor(newColor)
  selectedColor = colorNormalizado

  console.log(`🎨 CAMBIANDO COLOR A: ${colorNormalizado}`)
  console.log(`🏷️ CATEGORÍA ACTUAL: ${currentCategory}`)

  // Usar la categoría actual que ya fue detectada
  await applyLens(currentCategory, colorNormalizado)

  // Actualizar UI
  updateColorSelection(colorNormalizado)
}

// Función para cargar producto seleccionado (COMPLETAMENTE REESCRITA)
function loadSelectedProduct(): void {
  const productData = localStorage.getItem("prendaSeleccionada")

  if (productData) {
    try {
      selectedProduct = JSON.parse(productData)
      console.log("📦 PRODUCTO CARGADO:", selectedProduct)

      // Actualizar información del producto en la UI
      updateProductInfo(selectedProduct)

      // DETECTAR CATEGORÍA DE FORMA ROBUSTA
      currentCategory = detectarCategoriaProducto(selectedProduct)
      console.log(`🎯 CATEGORÍA FINAL DETECTADA: ${currentCategory}`)

      // Aplicar lente inicial con la categoría correcta
      console.log(`🚀 APLICANDO LENTE INICIAL: ${currentCategory} - ${selectedColor}`)
      applyLens(currentCategory, selectedColor)

      // Limpiar localStorage
      localStorage.removeItem("prendaSeleccionada")
    } catch (error) {
      console.error("❌ Error parseando producto seleccionado:", error)
      currentCategory = "Camisetas"
      applyLens(currentCategory, selectedColor)
    }
  } else {
    console.log("ℹ️ No hay producto seleccionado, usando configuración por defecto")
    currentCategory = "Camisetas"
    applyLens(currentCategory, selectedColor)
  }
}

// Función para actualizar información del producto en la UI
function updateProductInfo(product: any): void {
  const productInfoElement = document.getElementById("productInfo")
  const productNameElement = document.getElementById("productName")
  const productPriceElement = document.getElementById("productPrice")

  if (productInfoElement && productNameElement && productPriceElement) {
    productNameElement.textContent = product.nombre || "Producto"

    const precio = product.precio || 0
    const precioFormateado = new Intl.NumberFormat("es-CO", {
      style: "currency",
      currency: "COP",
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(precio)

    productPriceElement.textContent = precioFormateado
    productInfoElement.style.display = "block"
  }
}

// Función para actualizar selección de color en la UI
function updateColorSelection(color: ColorType): void {
  const colorDots = document.querySelectorAll(".color-dot")

  colorDots.forEach((dot) => {
    const element = dot as HTMLElement
    element.classList.remove("selected")

    // Mapear colores visuales a nombres
    const dotColor = element.dataset.color || ""
    const colorName = getColorNameFromHex(dotColor)

    if (colorName === color) {
      element.classList.add("selected")
    }
  })
}

// Función para mapear colores hex a nombres usando Record
function getColorNameFromHex(hexColor: string): ColorType {
  const colorMap: Record<string, ColorType> = {
    "#ffffff": "blanco",
    "#cccccc": "blanco",
    "#f8f8f8": "blanco",
    "#000000": "negro",
    "#333333": "negro",
    "#7a2e2e": "rojo",
    "#ff0000": "rojo",
    "#dc3545": "rojo",
  }

  const normalizedHex = hexColor.toLowerCase()
  return colorMap[normalizedHex] || "blanco"
}

// Función para mostrar notificación (MEJORADA)
function showLensNotification(message: string, type: "success" | "error" | "loading" = "success"): void {
  const notification = document.getElementById("notification")
  if (!notification) return

  const span = notification.querySelector("span")
  const icon = notification.querySelector("i")

  if (span) span.textContent = message

  // Cambiar icono según el tipo
  if (icon) {
    icon.className =
      type === "success"
        ? "fas fa-check-circle"
        : type === "error"
          ? "fas fa-exclamation-circle"
          : "fas fa-spinner fa-spin"
  }

  // Cambiar color según el tipo
  notification.style.background =
    type === "success"
      ? "linear-gradient(135deg, #28a745 0%, #20c997 100%)"
      : type === "error"
        ? "linear-gradient(135deg, #dc3545 0%, #c82333 100%)"
        : "linear-gradient(135deg, #007bff 0%, #0056b3 100%)"

  notification.classList.add("show")

  // Auto-hide para success y error
  if (type !== "loading") {
    setTimeout(
      () => {
        notification.classList.remove("show")
        // Resetear estilo
        setTimeout(() => {
          notification.style.background = ""
        }, 300)
      },
      type === "error" ? 4000 : 2000,
    )
  }
}

// Función principal de inicialización
async function initializeCameraKit(): Promise<void> {
  try {
    console.log("🚀 Inicializando CameraKit...")

    // Inicializar CameraKit
    cameraKit = await bootstrapCameraKit({
      apiToken:
        "eyJhbGciOiJIUzI1NiIsImtpZCI6IkNhbnZhc1MyU0hNQUNQcm9kIiwidHlwIjoiSldUIn0.eyJhdWQiOiJjYW52YXMtY2FudmFzYXBpIiwiaXNzIjoiY2FudmFzLXMyc3Rva2VuIiwibmJmIjoxNzQ3OTcyNjYwLCJzdWIiOiI0MGFkMTIwMC1lMzBiLTRhZGEtYTk2Yi04YzhhYjc2Yzg3OWN-U1RBR0lOR34yNmZiZjMwYi02MDEyLTQxMTQtYTYyZS1iMDg1ZDMzOGViNGEifQ.Aqc9IChl6LbBFHVBY8G2tRwRA8-CehoPHAA7ocW9LqQ",
    })

    // Configurar canvas
    const liveRenderTarget = document.getElementById("canvas") as HTMLCanvasElement
    if (!liveRenderTarget) {
      throw new Error("Canvas element not found")
    }

    // Crear sesión
    session = await cameraKit.createSession({ liveRenderTarget })

    // Obtener acceso a la cámara
    mediaStream = await navigator.mediaDevices.getUserMedia({
      video: { facingMode: "user" },
    })

    // Configurar fuente de video
    await session.setSource(mediaStream)
    await session.play()

    console.log("✅ CameraKit inicializado correctamente")

    // Ocultar loading
    const loadingElement = document.getElementById("loading")
    if (loadingElement) {
      loadingElement.style.display = "none"
    }

    // Cargar producto seleccionado y aplicar lente
    loadSelectedProduct()
  } catch (error) {
    console.error("❌ Error inicializando CameraKit:", error)

    const loadingElement = document.getElementById("loading")
    if (loadingElement) {
      loadingElement.innerHTML = `
        <h2>Error</h2>
        <p>No se pudo acceder a la cámara</p>
        <p>${error}</p>
      `
    }
  }
}

// Event listeners para la UI
document.addEventListener("DOMContentLoaded", () => {
  console.log("🎬 DOM cargado, inicializando...")

  // Inicializar CameraKit
  initializeCameraKit()

  // Event listeners para cambio de color
  const colorDots = document.querySelectorAll(".color-dot")
  colorDots.forEach((dot) => {
    dot.addEventListener("click", (event) => {
      const target = event.target as HTMLElement
      const hexColor = target.dataset.color || "#ffffff"
      const colorName = getColorNameFromHex(hexColor)

      console.log(`🎨 Color seleccionado: ${colorName} (${hexColor})`)
      changeColor(colorName)
    })
  })

  // Event listener para botón de captura
  const captureBtn = document.getElementById("captureBtn")
  if (captureBtn) {
    captureBtn.addEventListener("click", () => {
      capturePhoto()
    })
  }

  // Event listener para botón volver
  const volverBtn = document.getElementById("volverBtn")
  if (volverBtn) {
    volverBtn.addEventListener("click", () => {
      window.location.href = "catalogo.html"
    })
  }
})

// Función para capturar foto
function capturePhoto(): void {
  const canvas = document.getElementById("canvas") as HTMLCanvasElement
  if (!canvas) return

  try {
    // Crear enlace de descarga
    const link = document.createElement("a")
    link.download = `tryon-photo-${Date.now()}.png`
    link.href = canvas.toDataURL("image/png")
    link.click()

    console.log("📸 Foto capturada")
    showLensNotification("Foto capturada exitosamente", "success")
  } catch (error) {
    console.error("❌ Error capturando foto:", error)
    showLensNotification("Error al capturar la foto", "error")
  }
}

// Función para cambiar cámara (frontal/trasera)
async function toggleCamera(): Promise<void> {
  if (!mediaStream) return

  try {
    showLensNotification("Cambiando cámara...", "loading")

    // Detener stream actual
    mediaStream.getTracks().forEach((track) => track.stop())

    // Alternar entre frontal y trasera
    const videoTrack = mediaStream.getVideoTracks()[0]
    const currentFacingMode = videoTrack.getSettings().facingMode || "user"
    const newFacingMode = currentFacingMode === "user" ? "environment" : "user"

    // Obtener nuevo stream
    mediaStream = await navigator.mediaDevices.getUserMedia({
      video: { facingMode: newFacingMode },
    })

    // Actualizar sesión
    await session.setSource(mediaStream)

    console.log(`📱 Cámara cambiada a: ${newFacingMode}`)
    showLensNotification(`Cámara ${newFacingMode === "user" ? "frontal" : "trasera"} activada`, "success")
  } catch (error) {
    console.error("❌ Error cambiando cámara:", error)
    showLensNotification("Error al cambiar la cámara", "error")
  }
}

// Limpiar recursos al salir
window.addEventListener("beforeunload", () => {
  if (mediaStream) {
    mediaStream.getTracks().forEach((track) => track.stop())
  }
})

// Exponer funciones globalmente para uso en HTML
declare global {
  interface Window {
    changeColor: (color: string) => Promise<void>
    toggleCamera: () => Promise<void>
    capturePhoto: () => void
    applyLens: (categoria: string, color: string) => Promise<void>
    removeLens: () => Promise<void>
    forceChangeCategory: (categoria: CategoriaType) => Promise<void>
  }
}

window.changeColor = changeColor
window.toggleCamera = toggleCamera
window.capturePhoto = capturePhoto
window.applyLens = applyLens
window.removeLens = removeLens
window.forceChangeCategory = forceChangeCategory
