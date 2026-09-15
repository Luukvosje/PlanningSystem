/**
 * Whether text on a palette colour should be dark or light. The palette runs from #EAB308 to
 * #64748B, so a fixed white foreground is unreadable on half of it.
 */
export function readableTextColor(hexColor: string): string {
  const hex = hexColor.replace('#', '');

  if (hex.length !== 6) {
    return '#fff';
  }

  const red = parseInt(hex.slice(0, 2), 16);
  const green = parseInt(hex.slice(2, 4), 16);
  const blue = parseInt(hex.slice(4, 6), 16);

  // Rec. 709 luma: the cheap version of "is this colour light".
  const luma = (0.2126 * red + 0.7152 * green + 0.0722 * blue) / 255;

  return luma > 0.6 ? '#111827' : '#fff';
}

/** Up to two initials for an entity that has one name field instead of a first and last name. */
export function initialsFromName(name: string): string {
  return name
    .split(/\s+/)
    .filter((part) => /[\p{L}\p{N}]/u.test(part))
    .slice(0, 2)
    .map((part) => part.charAt(0))
    .join('')
    .toUpperCase();
}
