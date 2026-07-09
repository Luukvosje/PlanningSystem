---
name: Executive Precision
colors:
  surface: '#f8f9ff'
  surface-dim: '#cbdbf5'
  surface-bright: '#f8f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#eff4ff'
  surface-container: '#e5eeff'
  surface-container-high: '#dce9ff'
  surface-container-highest: '#d3e4fe'
  on-surface: '#0b1c30'
  on-surface-variant: '#45464d'
  inverse-surface: '#213145'
  inverse-on-surface: '#eaf1ff'
  outline: '#76777d'
  outline-variant: '#c6c6cd'
  surface-tint: '#565e74'
  primary: '#000000'
  on-primary: '#ffffff'
  primary-container: '#131b2e'
  on-primary-container: '#7c839b'
  inverse-primary: '#bec6e0'
  secondary: '#006a61'
  on-secondary: '#ffffff'
  secondary-container: '#86f2e4'
  on-secondary-container: '#006f66'
  tertiary: '#000000'
  on-tertiary: '#ffffff'
  tertiary-container: '#191c1e'
  on-tertiary-container: '#818486'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dae2fd'
  primary-fixed-dim: '#bec6e0'
  on-primary-fixed: '#131b2e'
  on-primary-fixed-variant: '#3f465c'
  secondary-fixed: '#89f5e7'
  secondary-fixed-dim: '#6bd8cb'
  on-secondary-fixed: '#00201d'
  on-secondary-fixed-variant: '#005049'
  tertiary-fixed: '#e0e3e5'
  tertiary-fixed-dim: '#c4c7c9'
  on-tertiary-fixed: '#191c1e'
  on-tertiary-fixed-variant: '#444749'
  background: '#f8f9ff'
  on-background: '#0b1c30'
  surface-variant: '#d3e4fe'
typography:
  display:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.01em
  headline-lg-mobile:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  headline-md:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  label-md:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.05em
  label-sm:
    fontFamily: Inter
    fontSize: 11px
    fontWeight: '500'
    lineHeight: 14px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 8px
  xs: 4px
  sm: 12px
  md: 24px
  lg: 40px
  xl: 64px
  container-max: 1440px
  gutter: 24px
---

## Brand & Style
The design system is engineered for high-performance planning environments where clarity and cognitive ease are paramount. The brand personality is professional, reliable, and meticulously organized, catering to project managers and teams who require a distraction-free workspace. 

The aesthetic follows a **Modern Minimalist** movement, utilizing generous whitespace to de-clutter complex data sets. To prevent the interface from feeling sterile, we incorporate subtle depth through layered surfaces and intentional accent pops. The goal is to evoke a sense of "structured calm," ensuring users feel in control of their workflows regardless of task volume.

## Colors
The palette is rooted in a professional "Deep Navy" (#0F172A) used for primary branding and high-level navigation, providing a grounded foundation. "Vibrant Teal" (#0D9488) serves as the primary action and accent color, guiding the eye toward progress and completion.

The background uses a "Soft Slate" (#F8FAFC) to reduce eye strain compared to pure white, creating a distinct "workspace" feel that separates the canvas from UI elements. Neutral grays are used strictly for secondary information and borders, maintaining a high-contrast ratio for accessibility while ensuring the interface remains visually quiet.

## Typography
This design system utilizes **Inter** exclusively to leverage its exceptional legibility in data-dense SaaS environments. The type scale is systematic, prioritizing vertical rhythm and clear hierarchy.

- **Display & Headlines:** Use tight letter-spacing and heavier weights to create a sense of authority. 
- **Body Text:** Set with generous line-heights to facilitate scanning of long-form project descriptions or task lists.
- **Labels:** Small caps and increased tracking are used for metadata, status tags, and table headers to distinguish them from actionable content.

## Layout & Spacing
The layout relies on a **Fluid Grid** with a 12-column structure for desktop and a single-column stack for mobile. We employ an 8px linear scale for all spacing tokens to ensure mathematical harmony across the UI.

- **Desktop (1280px+):** 12 columns, 24px gutters, 40px side margins.
- **Tablet (768px - 1279px):** 6 columns, 20px gutters, 24px side margins.
- **Mobile (Up to 767px):** 2 columns, 16px gutters, 16px side margins.

Content is organized into logical "zones." Navigation is typically docked in a slim left-hand sidebar (240px) to maximize the horizontal space available for Gantt charts, Kanban boards, and tables.

## Elevation & Depth
Elevation is conveyed through **Tonal Layers** and **Ambient Shadows**. We avoid heavy black shadows in favor of soft, diffused blurs tinted with the Deep Navy primary color at very low opacity (4-8%).

1. **Level 0 (Base):** Soft Slate (#F8FAFC) background.
2. **Level 1 (Cards/Sidebar):** Pure White (#FFFFFF) with a 1px border (#E2E8F0).
3. **Level 2 (Dropdowns/Modals):** Pure White with a medium ambient shadow (0px 10px 15px -3px rgba(15, 23, 42, 0.08)).
4. **Interactive (Hover):** Elements slightly lift using a subtle transform and increased shadow intensity to provide tactile feedback.

## Shapes
The design system uses a **Rounded** (Level 2) shape language to soften the professional aesthetic and make the workspace feel more approachable. 

- **Standard Buttons & Inputs:** 8px (0.5rem) corner radius.
- **Cards & Containers:** 16px (1rem) corner radius.
- **Modals & Large Sheets:** 24px (1.5rem) corner radius.
- **Progress Bars & Tags:** Fully rounded (pill-shaped) to distinguish them from structural containers.

## Components
Consistent styling of core elements ensures the planning experience is intuitive:

- **Buttons:** Primary buttons use the Teal accent with white text. Secondary buttons use a transparent background with a 1px Slate border. All buttons feature a 150ms transition on hover.
- **Cards:** Clean, white surfaces with no borders; depth is provided by a subtle Level 1 shadow. Padding inside cards is strictly 24px.
- **Progress Bars:** Use a dual-tone Teal approach—a light 10% opacity Teal for the track and 100% Teal for the fill.
- **Status Indicators:** Small, circular dots paired with Label-SM typography. Colors follow semantic logic: Green (Done), Teal (In Progress), Amber (Pending), Red (Blocked).
- **Input Fields:** Use a 1px Slate border that transitions to Teal on focus. Labels are always positioned above the input in Label-MD style.
- **Lists:** High-density rows with subtle 1px dividers. Hover states use a Soft Slate tint to highlight the active row without visual noise.