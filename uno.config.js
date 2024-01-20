// uno.config.ts
import { defineConfig } from 'unocss'
import presetWind from '@unocss/preset-wind'
import transformerDirectives from '@unocss/transformer-directives'

export default defineConfig({
    transformers: [
        transformerDirectives(),
    ],
    presets: [
        presetWind(),
    ],
})