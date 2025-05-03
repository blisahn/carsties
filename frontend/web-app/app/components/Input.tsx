import { HelperText, Label, TextInput } from 'flowbite-react'
import React from 'react'
import { useController, UseControllerProps } from 'react-hook-form'

type Props = {
    label: string,
    type?: string,
    showLabel?: boolean
} & UseControllerProps
export default function Input(props: Props) {
    const { fieldState, field } = useController({ ...props, defaultValue: '' })
    return (
        <div className='mb-3'>
            {props.showLabel && (
                <div className='mb-2 block'>
                    <Label htmlFor={field.name} defaultValue={props.label} />
                </div>
            )}
            <TextInput
                {...props}
                {...field}
                type={props.type || 'text'}
                placeholder={props.label}
                color={fieldState.error ? 'failure' : !fieldState.isDirty ? '' : 'success'}
            />
            <HelperText>
                {fieldState.error?.message}
            </HelperText>
        </div>
    )
}
