// @ts-check
import withNuxt from './.nuxt/eslint.config.mjs';

export default withNuxt({
	rules: {
		'@typescript-eslint/no-explicit-any': 'error',
		'no-eval': ['error'],
		'no-var': ['error'],
		'curly': ['error', 'all'],
		'semi': ['error', 'always'],
		'quotes': ['error', 'single'],
		'brace-style': 'error',
		'object-curly-spacing': ['error', 'always'],
		'@typescript-eslint/no-unused-expressions': 'off',
		'@typescript-eslint/unified-signatures': 'off',
		'comma-dangle': ['error', {
			'arrays': 'always-multiline',
			'objects': 'always-multiline',
			'imports': 'always-multiline',
			'exports': 'always-multiline',
			'functions': 'always-multiline',
		}],
		'vue/html-indent': ['error', 'tab'],
		'vue/multi-word-component-names': 'off',
		'vue/max-attributes-per-line': ['error', {
			'singleline': 1,
			'multiline': { 'max': 1 },
		}],
		'vue/singleline-html-element-content-newline': ['error', {
			'ignoreWhenNoAttributes': false,
			'ignoreWhenEmpty': false,
		}],
		'vue/no-v-html': ['off', {
			'ignorePattern': '^html',
		}],
		'vue/require-default-prop': 'error',
		'vue/html-closing-bracket-newline': ['error'],
		'vue/html-closing-bracket-spacing': ['error'],
		'arrow-parens': ['error', 'always'],
		'no-console': ['warn', { allow: ['info', 'warn', 'error'] }],
		'prefer-const': ['error'],
		'no-extra-semi': 'error',
		'operator-linebreak': ['error', 'after'],
		'no-nested-ternary': 'error',
		'import/no-duplicates': 'warn',
	},
	ignores: [
		'app/shared/**',
		'!pages/public/**/*.vue',
	],
});
