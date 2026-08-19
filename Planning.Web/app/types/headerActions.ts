/** Mirrors the alias list in nuxt.config.ts (`ui.theme.colors`) plus the always-present neutral. */
export type HeaderActionColor =
  | 'brand'
  | 'success'
  | 'info'
  | 'warning'
  | 'error'
  | 'neutral'

export interface HeaderActionSelectOption {
  label: string
  value: string
  icon?: string
}

interface HeaderActionBase {
  key: string
}

export interface HeaderActionButton extends HeaderActionBase {
  type: 'button'
  label: string
  icon?: string
  color?: HeaderActionColor
  onSelect: () => void
}

export interface HeaderActionSwitch extends HeaderActionBase {
  type: 'switch'
  label: string
  description?: string
  icon?: string
  modelValue: boolean
  onUpdate: (value: boolean) => void
}

export interface HeaderActionSelect extends HeaderActionBase {
  type: 'select'
  label: string
  value: string
  items: HeaderActionSelectOption[]
  onUpdate: (value: string) => void
  desktop?: 'buttons' | 'menu'
}

export interface HeaderActionPopover extends HeaderActionBase {
  type: 'popover'
  label: string
  icon?: string
}

export interface HeaderActionSlot extends HeaderActionBase {
  type: 'slot'
}

export type HeaderAction =
  | HeaderActionButton
  | HeaderActionSwitch
  | HeaderActionSelect
  | HeaderActionPopover
  | HeaderActionSlot
