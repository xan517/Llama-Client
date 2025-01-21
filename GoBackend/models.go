// models.go
package main

import (
    "time"

    "gorm.io/gorm"
)

// Chat represents a chat message between user and assistant
type Chat struct {
    ID          uint           `gorm:"primaryKey" json:"id"`
    UserMessage string         `gorm:"type:text;not null" json:"user_message"`
    BotResponse string         `gorm:"type:text;not null" json:"bot_response"`
    CreatedAt   time.Time      `json:"created_at"`
    UpdatedAt   time.Time      `json:"updated_at"`
    DeletedAt   gorm.DeletedAt `gorm:"index" json:"-"`
}