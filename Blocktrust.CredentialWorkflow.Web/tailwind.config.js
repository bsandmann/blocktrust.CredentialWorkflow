/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ['./**/*.{razor,html}'],
    safelist: [
    ],
    theme: {
        extend: {
            width: {
                102: '25.5rem', // 408px
                106: '26rem', // 426px
                112: '28rem', // 448px
                120: '30rem', // 480px
                128: '32rem', // 512px
                136: '34rem', // 544px
                144: '36rem', // 576px
            },
            fontFamily: {
                // Override the default font stack
                'sans': ['museo-sans', 'system-ui', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'Helvetica Neue', 'Arial', 'Noto Sans', 'sans-serif', 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', 'Noto Color Emoji'],
                'museo': ['museo', 'serif'],
                'museo-sans': ['museo-sans', 'sans-serif'],
                'code': ['source-code-variable', 'sans-serif'],
                'poppins': ['Poppins', 'sans-serif'],
            }
        }
    },
    variants: {
        extend: {
            borderColor: ['focus']
        }
    },
    plugins: [
    ]
}