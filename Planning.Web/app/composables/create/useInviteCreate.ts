import FormCreateInviteExtra from '~/components/form/create/InviteExtra.vue'
import { useCreate } from '~/lib/form/useCreate'
import { createInviteSchema } from '~/schemas/invite.schema'

const INVITE_CREATE_KEY = Symbol('invite-create')

export function useInviteCreate() {
  const invitesApi = useInvitesApi()
  const toast = useToast()
  const createdInvite = ref<{ code: string, expiresAtUtc: string } | null>(null)

  return useCreate(INVITE_CREATE_KEY, {
    title: 'Team uitnodigen',
    description: 'Genereer een code en deel deze met je teamlid. De code is 24 uur geldig.',
    schema: createInviteSchema,
    initialState: {},
    controls: [],
    submitLabel: 'Code genereren',
    footerMode: 'close-only',
    closeOnSuccess: false,
    bodyExtra: FormCreateInviteExtra,
    extensions: { createdInvite },
    onSubmit: async () => {
      const response = await invitesApi.create({})

      createdInvite.value = {
        code: response.code ?? '',
        expiresAtUtc: response.expiresAtUtc ?? '',
      }

      toast.add({ title: 'Uitnodiging aangemaakt', color: 'success' })
    },
    onClose: () => {
      createdInvite.value = null
    },
  })
}
